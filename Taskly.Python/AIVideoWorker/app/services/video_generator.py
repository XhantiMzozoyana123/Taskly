"""AI TEXT-to-video generation using Wan2.1 (only supported model).

Supports animating a single image with a diffusion model.  The active model is
selected by ``settings.AI_MODEL``: **Wan2.1 T2V-1.3B** is the only backend
in this build.  When
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

# Heavy pipelines are loaded once and cached per-model (wan) so that
# subsequent jobs reuse them instead of re-downloading the weights.
_pipeline_cache: dict = {}
_pipeline_lock = threading.Lock()

# Supported model id (Wan-only build).
WAN_MODEL_ID = "Wan-AI/Wan2.1-T2V-1.3B-Diffusers"


_ai_import_error: str = ""


def ai_available() -> bool:
    """True when torch + diffusers + Pillow are importable (i.e. a model can run)."""
    global _ai_import_error
    try:
        import diffusers  # noqa: F401
        import torch  # noqa: F401
        from PIL import Image  # noqa: F401
        return True
    except Exception as exc:  # pylint: disable=broad-except
        _ai_import_error = f"{type(exc).__name__}: {exc}"
        return False


def deployment_status() -> dict:
    """Report what the app can run; surfaced by /health for quick troubleshooting.

    Catches import errors so a broken ML install can never take /health down.
    """
    status = {
        "available": ai_available(),
        "import_error": _ai_import_error or None,
        "ai_model": settings.AI_MODEL,
        "tour_style": settings.TOUR_STYLE,
        "device": settings.AI_DEVICE,
    }
    try:
        import torch

        status["torch_version"] = torch.__version__
        status["torch_cuda_available"] = bool(torch.cuda.is_available())
        if torch.cuda.is_available():
            status["torch_cuda_device"] = torch.cuda.get_device_name(0)
    except Exception as exc:  # pylint: disable=broad-except
        status["torch_error"] = f"{type(exc).__name__}: {exc}"
    try:
        import diffusers

        status["diffusers_version"] = diffusers.__version__
    except Exception as exc:  # pylint: disable=broad-except
        status["diffusers_error"] = f"{type(exc).__name__}: {exc}"
    return status


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


def _load_wan():
    """Load the Wan2.1 text-to-video pipeline.

    Uses diffusers' auto ``DiffusionPipeline`` (which resolves
    ``Wan-AI/Wan2.1-T2V-1.3B`` to the registered Wan2.1 T2V pipeline) instead of
    importing ``WanT2VPipeline`` directly -- the pipeline class name differs across
    diffusers releases (``WanT2VPipeline`` in 0.30/0.31, ``WanPipeline`` on main),
    so the auto class is the only import that is stable across versions.

    The 1.3B weights ship as a single fp16 ``diffusion_pytorch_model.safetensors``
    (the default file), so NO ``variant`` is passed: requesting
    ``variant="fp16"`` makes ``from_pretrained`` raise
    "does not have a file named ...diffusion_pytorch_model.fp16.safetensors".
    The 1.3B variant runs in fp16 within ~11 GB, fitting the RTX A4000 (16 GB);
    the 4.5B variant needs ~24 GB and is intentionally not used here.

    Requires diffusers >= 0.33 and the gated ``Wan-AI/Wan2.1-T2V-1.3B-Diffusers``
    weights (accept the license on Hugging Face and set ``AI_LTX_MODEL_ID`` to it).
    The ``-Diffusers`` repo's ``model_index.json`` declares the ``WanPipeline``
    class (added in diffusers 0.33; 0.30/0.31 only shipped ``WanT2VPipeline``).
    """
    import torch
    from diffusers import DiffusionPipeline

    logger.info("Loading Wan2.1 model %s ...", settings.AI_LTX_MODEL_ID)
    pipe = DiffusionPipeline.from_pretrained(
        settings.AI_LTX_MODEL_ID,
        torch_dtype=torch.float16,
    )
    if settings.AI_DEVICE == "cuda":
        # Wan2.1-T2V-1.3B needs only ~8 GB VRAM (per the official repo), so the
        # whole pipeline stays RESIDENT on cuda. enable_model_cpu_offload() drags
        # convs across the cuda<->CPU boundary -> cuDNN NOT_INITIALIZED here.
        pipe.to("cuda")
        logger.info("Wan2.1 pipeline resident on cuda (~8 GB fp16)")
    else:
        pipe.to("cpu")
        logger.info("Wan2.1 pipeline on CPU (inference will be slow)")
    return pipe


_MODEL_LOADERS = {
    "wan": _load_wan,
}


def _get_pipeline(model: str = "wan"):
    """Load (once) and return the cached pipeline for the requested model.

    `Only ``"wan"`` (Wan2.1 T2V) is supported in this build.
    Each pipeline is loaded and cached independently, so switching models only
    pays the load cost once.
    """
    model = (model or "wan").lower()
    if model in _pipeline_cache:
        return _pipeline_cache[model]

    with _pipeline_lock:
        if model in _pipeline_cache:
            return _pipeline_cache[model]

        loader = _MODEL_LOADERS.get(model)
        if loader is None:
            raise ValueError(f"Unknown AI model '{model}'. This build supports 'wan' only.")

        pipe = loader()
        _pipeline_cache[model] = pipe
        logger.info("%s pipeline ready on %s", model.upper(), settings.AI_DEVICE)
        return pipe


def _wan_num_frames(num_frames: int) -> int:
    """Snap a requested frame count to Wan2.1's supported grid.

    Wan2.1 T2V only accepts fixed frame counts (21/41/61/81/121/145/201); we
    round UP to the first supported value so short clips run fast and long
    clips stay valid.
    """
    try:
        n = int(num_frames or 0)
    except (TypeError, ValueError):
        n = 0
    grid = (21, 41, 61, 81, 121, 145, 201)
    for cand in grid:
        if cand >= n:
            return cand
    return grid[-1]


def _wan_resolution(width: int, height: int):
    """Return a Wan2.1-supported (width, height) closest to the request.

    Wan2.1 is trained natively at 720p.  Both dimensions are clamped to
    16-pixel boundaries (the model VAE/patch grid) and floored at 320 px; the
    1.3B checkpoint fits comfortably in 16 GB of VRAM at this resolution.
    """
    w = max(320, int(width or 1280))
    h = max(320, int(height or 720))
    return (w // 16) * 16, (h // 16) * 16


def _generate_text_with_wan(prompt: str, num_frames: int, width: int, height: int):
    """Run the diffusers Wan2.1 T2V pipeline for pure TEXT-to-video.

    Wan2.1 (1.3B, native 720p, ~50 default denoising steps, strong prompt
    adherence) is the closest local alternative to Google Veo on a 16 GB GPU.
    Resolution and frame count are snapped to supported values; any failure
    raises so the worker marks the job failed with the Wan traceback.
    """
    pipe = _get_pipeline("wan")
    n = _wan_num_frames(num_frames)
    w, h = _wan_resolution(width, height)
    result = pipe(
        prompt=prompt,
        num_frames=n,
        height=h,
        width=w,
        generator=settings.generator(),
    )
    frames = getattr(result, "frames", None)
    if frames is None and isinstance(result, (tuple, list)) and result:
        frames = result[0]
    if not frames:
        raise RuntimeError("Wan2.1 pipeline returned no frames")
    frames = frames[0]
    return frames, w, h


def _pil_from_bytes(data: bytes) -> "Image.Image":
    """Decode raw image bytes into an ``RGB`` PIL image.

    Pillow's ``Image.open`` is strict about the file header, whereas OpenCV's
    decoder re-syncs on image markers and is far more tolerant of slightly
    corrupt bytes (for example the artefacts produced when URL-safe base64
    is decoded with the standard alphabet, which is exactly what used to make
    the AI tour raise ``UnidentifiedImageError`` while the Ken Burns fallback --
    which decodes via OpenCV first -- silently succeeded).

    We therefore try Pillow first and fall back to OpenCV, mirroring
    ``slideshow._bgr_from_any`` so the AI tour accepts every image the
    Ken Burns fallback accepts (and vice-versa).
    """
    from PIL import Image

    try:
        return Image.open(io.BytesIO(data)).convert("RGB")
    except Exception:
        import cv2
        import numpy as np

        arr = np.frombuffer(data, dtype=np.uint8)
        img = cv2.imdecode(arr, cv2.IMREAD_COLOR)
        if img is None:
            raise ValueError(
                "cannot decode image bytes: neither Pillow nor OpenCV "
                "recognised the image format"
            )
        img = cv2.cvtColor(img, cv2.COLOR_BGR2RGB)
        return Image.fromarray(img)


def generate_image_to_video(*args, **kwargs):
    """Disabled in the Wan-only build (Wan I2V is 14B, ~24 GB+ VRAM)."""
    raise NotImplementedError(
        "Image-to-video is disabled in this Wan-only build: Wan2.1 I2V ships "
        "only as the 14B model (~24 GB+ VRAM). Use text-to-video instead."
    )


def compute_t2v_frames(duration_seconds: int, fps: int) -> int:
    """Reasonable frame count on the diffusion-friendly 8-1 grid."""
    n = int(duration_seconds) * max(fps, 1)
    return max(9, (n // 8) * 8 + 1)
def generate_text_to_video(
    prompt: str,
    output_path: Path,
    *,
    duration_seconds: int = 4,
    width: int = 1280,
    height: int = 720,
    fps: int = 25,
    seed: int = 0,
) -> Path:
    """Generate a video from a TEXT prompt.

    Generates a video from TEXT only, using Wan2.1 T2V-1.3B (the sole backend
    in this Wan-only build).

    Returns the path to the generated MP4 file.
    """
    num_frames = compute_t2v_frames(duration_seconds, int(fps))

    if not ai_available():
        raise RuntimeError("No text-to-video backend: diffusers+torch missing.")
    logger.info("Using Wan2.1 T2V (only backend in this Wan-only build) ...")
    try:
        frames, w, h = _generate_text_with_wan(prompt, num_frames, width, height)
        return write_frames_video(frames, output_path, fps=fps, width=w, height=h)
    except Exception:
        logger.error("Wan2.1 generation failed; marking job as failed.", exc_info=True)
        raise


def _stitch_clips(clips, canvas_w, canvas_h, transition_duration, fps):
    """Crossfade a list of clips (each a list of PIL frames) into one sequence.

    All frames are normalised to ``canvas_w`` x ``canvas_h`` and the tail of each
    clip is blended with the head of the next, so every source frame appears
    exactly once.  Returns BGR uint8 frames ready for encoding.
    """
    import cv2
    import numpy as np
    from PIL import Image

    def _to_bgr(pil_img):
        arr = np.array(
            pil_img.convert("RGB").resize((canvas_w, canvas_h), Image.LANCZOS)
        )
        return arr[:, :, ::-1].copy()

    resized = [[_to_bgr(f) for f in clip] for clip in clips]
    if not resized:
        return []
    n = len(resized)
    lens = [len(c) for c in resized]
    min_len = max(1, min(lens))
    requested = max(1, int(round(transition_duration * fps)))
    t = min(requested, max(1, min_len // 2))

    out = []
    for i in range(n):
        length = lens[i]
        start = t if i > 0 else 0
        end = length - t if i < n - 1 else length
        if end > start:
            out.extend(resized[i][start:end])
        if i < n - 1:
            nxt = resized[i + 1]
            nf = min(t, length - end, len(nxt))
            for k in range(nf):
                a = resized[i][end + k] if end + k < length else resized[i][-1]
                b = nxt[k]
                alpha = (k + 1) / (nf + 1)
                out.append(cv2.addWeighted(a, 1.0 - alpha, b, alpha, 0))
    return out


def generate_image_sequence_video(*args, **kwargs):
    """Disabled in the Wan-only build; caller falls back to Ken Burns slideshow."""
    raise NotImplementedError(
        "AI photo tours are disabled in this Wan-only build; the caller falls "
        "back to the Ken Burns slideshow."
    )


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
