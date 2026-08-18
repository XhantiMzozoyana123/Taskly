"""AI image-to-video generation using HuggingFace models.

Supports animating a single image with a diffusion model.  The active model is
selected by ``settings.AI_MODEL``: by default **Lightricks LTX-Video** (fast,
OpenArt-style motion), with **Stable Video Diffusion** as an alternative.  When
torch/diffusers are unavailable the caller falls back to the Ken Burns
slideshow in :mod:`app.services.slideshow`.

The heavy ML dependencies (torch, diffusers, Pillow) are imported lazily
inside the functions so that the rest of the application can be imported and
run even when no GPU / CUDA toolkit is present.
"""
import io
import logging
import threading
from fractions import Fraction
from pathlib import Path

from app.core.config import settings

logger = logging.getLogger("aivideoworker.video_generator")

# Heavy pipelines are loaded once and cached per-model (svd / ltx) so that
# subsequent jobs reuse them instead of re-downloading the weights.
_pipeline_cache: dict = {}
_pipeline_lock = threading.Lock()

# Supported model IDs
SVD_MODEL_ID = "stabilityai/stable-video-diffusion-img2vid"
LTX_MODEL_ID = "Lightricks/LTX-Video"


def ai_available() -> bool:
    """True when torch + diffusers + Pillow are importable (i.e. a model can run)."""
    try:
        import diffusers  # noqa: F401
        import torch  # noqa: F401
        from PIL import Image  # noqa: F401
        return True
    except Exception:
        return False


def model_available(model_id: str) -> bool:
    """Check if a specific model is available by trying to import its components."""
    try:
        import diffusers  # noqa: F401
        import torch  # noqa: F401
        from PIL import Image  # noqa: F401
        # If we can import these, the model should be available
        return True
    except Exception:
        return False


def _resize_for_svd(image) -> "Image.Image":
    """Resize an image so its dimensions are multiples of 64 (SVD requires it),
    keeping the aspect ratio and fitting within the configured max edge."""
    from PIL import Image  # lazy import keeps the module importable w/o Pillow

    width, height = image.size
    max_edge = settings.AI_MAX_EDGE
    scale = min(1.0, max_edge / max(width, height))

    new_width = max(64, int(width * scale) // 64 * 64)
    new_height = max(64, int(height * scale) // 64 * 64)

    if (new_width, new_height) != (width, height):
        image = image.resize((new_width, new_height), Image.LANCZOS)
    return image


def _load_svd():
    """Load and return the Stable Video Diffusion pipeline."""
    import torch
    from diffusers import StableVideoDiffusionPipeline

    logger.info("Loading Stable Video Diffusion model %s ...", settings.AI_MODEL_ID)
    dtype = torch.float16 if settings.AI_USE_FP16 else torch.float32

    pipe = StableVideoDiffusionPipeline.from_pretrained(
        settings.AI_MODEL_ID,
        torch_dtype=dtype,
        variant="fp16" if settings.AI_USE_FP16 else None,
    )

    if settings.AI_DEVICE == "cuda":
        pipe.to("cuda")
        # Optional optimizations - only apply when the pipeline supports them.
        if hasattr(pipe, "enable_vae_tiling"):
            pipe.enable_vae_tiling()
        if settings.AI_CPU_OFFLOAD and hasattr(pipe, "enable_model_cpu_offload"):
            pipe.enable_model_cpu_offload()
            logger.info(
                "CPU offload enabled - weights stream between CPU/GPU "
                "(may cause device mismatch with SVD fp16)"
            )
        else:
            logger.info(
                "CPU offload disabled - full pipeline stays on %s", settings.AI_DEVICE
            )
    else:
        pipe.to("cpu")
        logger.info("SVD pipeline on CPU (inference will be slow)")
    return pipe


def _load_ltx():
    """Load and return the Lightricks LTX-Video pipeline.

    LTX-Video is a DiT-based image-to-video model (T5 text encoder, loaded via
    diffusers). It is loaded in bfloat16 to keep VRAM low (~4 GB for the 2B
    checkpoint), which fits 8 GB+ GPUs.
    """
    import torch
    from diffusers import DiffusionPipeline

    logger.info("Loading LTX-Video model %s ...", settings.AI_LTX_MODEL_ID)
    pipe = DiffusionPipeline.from_pretrained(
        settings.AI_LTX_MODEL_ID,
        torch_dtype=torch.bfloat16,
    )

    if settings.AI_DEVICE == "cuda":
        pipe.to("cuda")
        if settings.AI_CPU_OFFLOAD and hasattr(pipe, "enable_model_cpu_offload"):
            pipe.enable_model_cpu_offload()
            logger.info("CPU offload enabled for LTX")
    else:
        pipe.to("cpu")
        logger.info("LTX pipeline on CPU (inference will be slow)")
    return pipe


# Map an AI_MODEL name to the function that loads it.
_MODEL_LOADERS = {
    "ltx": _load_ltx,
    "svd": _load_svd,
}


def _get_pipeline(model: str = "svd"):
    """Load (once) and return the cached pipeline for the requested model.

    ``model`` may be ``"svd"`` (Stable Video Diffusion) or ``"ltx"`` (LTX-Video).
    Each pipeline is loaded and cached independently, so switching models only
    pays the load cost once.
    """
    model = (model or "svd").lower()
    if model in _pipeline_cache:
        return _pipeline_cache[model]

    with _pipeline_lock:
        if model in _pipeline_cache:
            return _pipeline_cache[model]

        loader = _MODEL_LOADERS.get(model)
        if loader is None:
            raise ValueError(f"Unknown AI model '{model}'. Use 'svd' or 'ltx'.")

        pipe = loader()
        _pipeline_cache[model] = pipe
        logger.info("%s pipeline ready on %s", model.upper(), settings.AI_DEVICE)
        return pipe


def _resize_to_divisible(image, max_edge: int = 1216, divisor: int = 32):
    """Resize ``image`` so both dimensions are multiples of ``divisor`` (the
    LTX VAE requires divisible spatial dims), preserving aspect ratio and
    keeping the longest side <= ``max_edge``.
    """
    from PIL import Image  # lazy import keeps the module importable w/o Pillow

    width, height = image.size
    scale = min(1.0, max_edge / max(width, height))
    new_width = max(divisor, int(width * scale) // divisor * divisor)
    new_height = max(divisor, int(height * scale) // divisor * divisor)
    if (new_width, new_height) != (width, height):
        image = image.resize((new_width, new_height), Image.LANCZOS)
    return image


def _generate_with_svd(image):
    """Run Stable Video Diffusion on ``image``; return ``(frames, w, h)``."""
    pipe = _get_pipeline("svd")
    image = _resize_for_svd(image)
    logger.info(
        "SVD: generating %s frames at %s fps from image (%sx%s) ...",
        settings.AI_NUM_FRAMES,
        settings.AI_FPS,
        image.width,
        image.height,
    )
    frames = pipe(
        image=image,
        num_frames=settings.AI_NUM_FRAMES,
        decode_chunk_size=settings.AI_DECODE_CHUNK_SIZE,
        motion_bucket_id=settings.AI_MOTION_BUCKET_ID,
        noise_aug_strength=settings.AI_NOISE_AUG_STRENGTH,
        generator=settings.generator(),
    ).frames[0]
    return frames, image.width, image.height


def _generate_with_ltx(image, prompt: str):
    """Run LTX-Video on ``image`` (+ optional ``prompt``); return ``(frames, w, h)``.

    LTX is an image-conditioned DiT video model. ``prompt`` is optional -- when
    empty ``settings.AI_LTX_PROMPT`` is used as text conditioning. Output frames
    are returned at LTX's native (VAE-divisible) resolution.
    """
    pipe = _get_pipeline("ltx")
    image = _resize_to_divisible(image, max_edge=1216, divisor=32)
    text = prompt or settings.AI_LTX_PROMPT
    logger.info(
        "LTX: animating image (%sx%s) with prompt=%r steps=%s ...",
        image.width,
        image.height,
        text,
        settings.AI_LTX_NUM_INFERENCE_STEPS,
    )
    result = pipe(
        image=image,
        prompt=text,
        num_inference_steps=settings.AI_LTX_NUM_INFERENCE_STEPS,
        generator=settings.generator(),
    )
    frames = result.frames[0]
    return frames, image.width, image.height


def generate_image_to_video(
    image_bytes: bytes,
    output_path: Path,
    prompt: str = "",
    model: str = None,
    fps: int = None,
) -> Path:
    """Generate a video that brings ``image_bytes`` to life.

    The AI image-to-video model is selected via ``model`` (or
    ``settings.AI_MODEL`` when omitted):

    * ``"ltx"`` -> Lightricks LTX-Video (default, fast OpenArt-style motion)
    * ``"svd"`` -> Stable Video Diffusion

    LTX is tried first; if the selected model cannot be loaded or run (e.g. an
    incompatible diffusers version, missing weights, or OOM), the call
    transparently falls back to SVD so a video is still produced, and a warning
    is logged.  Returns the path to the written video file (container/codec
    chosen from the ``output_path`` extension).  Raises on failure.
    """
    from PIL import Image

    model = (model or settings.AI_MODEL).lower()
    image = Image.open(io.BytesIO(image_bytes)).convert("RGB")

    if fps is None:
        fps = settings.AI_LTX_FPS if model == "ltx" else settings.AI_FPS

    try:
        if model == "ltx":
            frames, width, height = _generate_with_ltx(image, prompt=prompt)
        else:
            frames, width, height = _generate_with_svd(image)
    except Exception:
        if model == "ltx":
            logger.warning(
                "LTX-Video generation failed; falling back to Stable Video "
                "Diffusion.",
                exc_info=True,
            )
            frames, width, height = _generate_with_svd(image)
        else:
            raise

    return write_frames_video(
        frames, output_path, fps=fps, width=width, height=height
    )


# Map a file extension to the FFmpeg container format name.
_CONTAINER_FORMATS = {
    ".mp4": "mp4",
    ".mov": "mov",
    ".m4v": "mp4",
    ".webm": "webm",
    ".mkv": "matroska",
    ".avi": "avi",
}

# Map a file extension to the preferred encoder codec(s), best-first.
_CODEC_PREFERENCES = {
    ".mp4": ("libx264", "mpeg4"),
    ".mov": ("libx264", "mpeg4"),
    ".m4v": ("libx264", "mpeg4"),
    ".webm": ("libvpx-vp9", "libvpx"),
    ".mkv": ("libx264", "mpeg4"),
    ".avi": ("mpeg4",),
}


def _select_encoder(ext: str) -> str:
    """Pick the best available encoder codec for the given file extension."""
    import av

    available = {c for c in av.codec.codecs_available}
    ext = ext.lower()
    preferences = _CODEC_PREFERENCES.get(ext, ("libx264", "mpeg4"))
    for codec in preferences:
        if codec in available:
            return codec
    for codec in available:
        if "mpeg4" in codec:
            return codec
    raise RuntimeError(f"No usable video encoder available for extension '{ext}'")


def write_frames_video(
    frames, output_path: Path, fps: int, width: int, height: int
) -> Path:
    """Write a sequence of frames to a video file.

    Accepts a list of PIL Images or BGR numpy arrays (BGR arrays are passed
    straight to PyAV with ``format="bgr24"``). The container/codec is chosen
    from the file extension:

      * ``.mp4`` / ``.mov`` -> H.264 (``libx264``, falls back to ``mpeg4``)
      * ``.webm`` / ``.mkv`` -> VP9 (``libvpx-vp9``, falls back to ``libvpx``)

    MP4/H.264 is the format most sharing platforms (Airbnb, OpenArt, social
    media) expect; WebM/VP9 is still supported for backward compatibility.
    Returns the path to the written file.
    """
    import av
    import numpy as np
    import cv2  # noqa: F401  (used below to normalise numpy frames to BGR)
    from PIL import Image

    path = Path(output_path)
    ext = path.suffix.lower()
    container_format = _CONTAINER_FORMATS.get(ext, ext.lstrip(".") or "mp4")
    codec = _select_encoder(ext)

    logger.info(
        "Writing %s video -> %s (codec=%s, %sx%s @ %sfps)",
        container_format, path, codec, width, height, fps,
    )

    container = av.open(str(path), mode="w", format=container_format)
    stream = container.add_stream(codec, rate=Fraction(fps, 1))
    stream.width = width
    stream.height = height
    stream.pix_fmt = "yuv420p"

    if codec == "libx264":
        stream.options = {"preset": "medium", "crf": "23", "pix_fmt": "yuv420p"}
    elif codec in ("libvpx", "libvpx-vp9"):
        stream.options = {"crf": "30", "b": "0", "pix_fmt": "yuv420p"}

    def _to_bgr(frame) -> "np.ndarray":
        """Normalise any supported frame into a contiguous BGR uint8 array."""
        if isinstance(frame, Image.Image):
            arr = np.array(frame.convert("RGB"))[:, :, ::-1]  # RGB -> BGR
            return np.ascontiguousarray(arr)

        arr = np.asarray(frame)
        if arr.dtype != np.uint8:
            max_val = arr.max() if arr.size else 0.0
            if float(max_val) <= 1.0:
                arr = (arr * 255).astype(np.uint8)
            else:
                arr = arr.astype(np.uint8)
        if arr.ndim == 2:  # greyscale -> BGR
            arr = cv2.cvtColor(arr, cv2.COLOR_GRAY2BGR)
        elif arr.shape[2] == 4:  # BGRA -> BGR
            arr = cv2.cvtColor(arr, cv2.COLOR_BGRA2BGR)
        return np.ascontiguousarray(arr)

    try:
        for frame in frames:
            video_frame = av.VideoFrame.from_ndarray(_to_bgr(frame), format="bgr24")
            for packet in stream.encode(video_frame):
                container.mux(packet)
        # Flush the encoder.
        for packet in stream.encode():
            container.mux(packet)
    finally:
        container.close()

    return path


def write_frames_webm(
    frames, output_path: Path, fps: int, width: int, height: int
) -> Path:
    """Backward-compatible alias for :func:`write_frames_video`.

    Kept so existing callers and any external consumers pinned to the old
    WebM name keep working regardless of the output extension.
    """
    return write_frames_video(frames, output_path, fps, width, height)