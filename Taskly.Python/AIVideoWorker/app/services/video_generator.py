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
        if settings.AI_CPU_OFFLOAD and hasattr(pipe, "enable_model_cpu_offload"):
            pipe.enable_model_cpu_offload()
            logger.info("CPU offload enabled for Wan2.1 (weights stream CPU<->GPU)")
        else:
            pipe.to("cuda")
            logger.info("Wan2.1 pipeline on cuda")
    else:
        pipe.to("cpu")
        logger.info("Wan2.1 pipeline on CPU (inference will be slow)")
    return pipe


def _load_hunyuan():
    """Load the diffusers HunyuanVideo text-to-video pipeline.

    The ~13 BF16 transformer does not fit a 16 GB card as one tensor, so we load
    it in bfloat16, the rest of the pipeline in float16, then call
    ``enable_model_cpu_offload()`` (streams weights CPU<->GPU) plus
    ``vae.enable_tiling()`` (tiles the 3D VAE decode).  This is the officially
    documented way to run HunyuanVideo on 16 GB.  Requires diffusers >= 0.33.
    """
    import torch
    from diffusers import HunyuanVideoPipeline, HunyuanVideoTransformer3DModel

    model_id = settings.AI_HUNYUAN_MODEL_ID
    logger.info("Loading HunyuanVideo model %s ...", model_id)
    transformer = HunyuanVideoTransformer3DModel.from_pretrained(
        model_id, subfolder="transformer", torch_dtype=torch.bfloat16
    )
    pipe = HunyuanVideoPipeline.from_pretrained(
        model_id, transformer=transformer, torch_dtype=torch.float16
    )
    # Memory savings that make HunyuanVideo fit in 16 GB.
    tiler = getattr(pipe.vae, "enable_tiling", None)
    if callable(tiler):
        tiler()
    if settings.AI_CPU_OFFLOAD and hasattr(pipe, "enable_model_cpu_offload"):
        pipe.enable_model_cpu_offload()
        logger.info("HunyuanVideo CPU offload enabled (weights stream CPU<->GPU)")
    elif settings.AI_DEVICE == "cuda":
        pipe.to("cuda")
        logger.info("HunyuanVideo pipeline on cuda")
    else:
        pipe.to("cpu")
        logger.info("HunyuanVideo pipeline on CPU (inference will be slow)")
    return pipe


# Map an AI_MODEL name to the function that loads it.
_MODEL_LOADERS = {
    "ltx": _load_ltx,
    "svd": _load_svd,
    "wan": _load_wan,
    "hunyuan": _load_hunyuan,
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
            raise ValueError(f"Unknown AI model '{model}'. Use 'svd', 'ltx', 'wan', or 'hunyuan'.")

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


def _set_ltx_guidance(pipe, scale: float) -> None:
    """Best-effort override of LTX's guidance_scale.

    Several diffusers builds expose ``guidance_scale`` as a read-only property
    (instance assignment raises AttributeError).  diffusers stores the actual
    value on the private ``_guidance_scale`` attribute that the property reads,
    so we set that.  If neither attribute is writable, the pipeline's default
    guidance is kept -- this call never raises.
    """
    for attr in ("_guidance_scale", "guidance_scale"):
        try:
            setattr(pipe, attr, float(scale))
        except Exception:
            pass
    # diffusers LTXVideoPipeline leaves text_encoder on CPU while the VAE/latents
    # are on cuda; with guidance_scale > 1 the denoising loop raises
    # "Expected all tensors to be on the same device, cpu and cuda:0" (index_select).
    # Move the text encoder onto the pipeline device so embeddings land on cuda.
    text_encoder = getattr(pipe, "text_encoder", None)
    if text_encoder is not None:
        try:
            text_encoder.to(pipe.device)
        except Exception:
            pass


def _run_ltx(pipe, kwargs: dict):
    """Call the LTX pipeline, applying the negative prompt only when the
    installed diffusers ``__call__`` accepts it (prevents a TypeError
    regression on builds whose LTX __call__ signature lacks ``negative_prompt``)."""
    if settings.AI_LTX_NEGATIVE_PROMPT:
        try:
            return pipe(**kwargs, negative_prompt=settings.AI_LTX_NEGATIVE_PROMPT)
        except TypeError:
            return pipe(**kwargs)
    return pipe(**kwargs)


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
    raises so the caller falls back to LTX and the worker always returns video.
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


def _hunyuan_num_frames(num_frames: int) -> int:
    """Snap a requested frame count to HunyuanVideo's 4k+1 grid.

    HunyuanVideo's 3D VAE works on frame counts of the form 4k+1 (61, 65, ...,
    129 default).  Snap to that grid so short clips run fast and long clips stay
    valid.
    """
    try:
        n = int(num_frames or 0)
    except (TypeError, ValueError):
        n = 0
    return max(5, ((n - 1) // 4) * 4 + 1)


def _hunyuan_resolution(width: int, height: int):
    """Return a HunyuanVideo-friendly (width, height) on 16-px boundaries.

    HunyuanVideo's space/temporal VAE patch grid uses multiples of 16; clamp each
    dim and floor it at 320 px so short requests stay valid.
    """
    w = max(320, int(width or 1280))
    h = max(320, int(height or 720))
    return (w // 16) * 16, (h // 16) * 16


def _generate_text_with_hunyuan(
    prompt: str, num_frames: int, width: int, height: int
):
    """Run the diffusers HunyuanVideo pipeline for pure TEXT-to-video.

    HunyuanVideo is a strong local candidate on 16 GB cards (its text encoder is
    far lighter than Wan's ~12 GB), and ``_load_hunyuan`` enabled model CPU
    offload + VAE tiling so it fits the RTX A4000.  Returns (frames, w, h).
    """
    pipe = _get_pipeline("hunyuan")
    n = _hunyuan_num_frames(num_frames)
    w, h = _hunyuan_resolution(width, height)
    text = prompt or settings.AI_LTX_PROMPT or "cinematic camera motion"
    logger.info(
        "HunyuanVideo t2v: prompt=%r steps=%s frames=%d %dx%d ...",
        text, settings.AI_LTX_NUM_INFERENCE_STEPS, n, w, h,
    )
    result = pipe(
        prompt=text,
        height=h,
        width=w,
        num_frames=n,
        num_inference_steps=settings.AI_LTX_NUM_INFERENCE_STEPS,
        generator=settings.generator(),
    )
    frames = getattr(result, "frames", None)
    if frames is None and isinstance(result, (tuple, list)) and result:
        frames = result[0]
    if not frames:
        raise RuntimeError("HunyuanVideo pipeline returned no frames")
    frames = frames[0]
    return frames, w, h


def _generate_text_with_ltx(prompt: str, num_frames: int = None, width: int = 1280, height: int = 720):
    """Run the diffusers Lightricks/LTX-Video pipeline for pure TEXT-to-video.

    Uses the same already-loaded diffusers LTX pipeline as the image tour
    (``_get_pipeline("ltx")``), but omits the ``image`` conditioning so LTX
    generates motion from the prompt alone.  Returns ``(frames, width, height)``
    at LTX's native VAE-divisible resolution.
        """
    pipe = _get_pipeline("ltx")
    text = prompt or settings.AI_LTX_PROMPT or "cinematic camera motion"
    _set_ltx_guidance(pipe, settings.AI_LTX_GUIDANCE_SCALE)
    kwargs = dict(
        prompt=text,
        # LTX needs a resolution even for text-to-video.
        width=_resize_to_divisible_width(width, divisor=32),
        height=_resize_to_divisible_height(height, divisor=32),
        num_inference_steps=settings.AI_LTX_NUM_INFERENCE_STEPS,
        generator=settings.generator(),
    )
    if num_frames:
        kwargs["num_frames"] = num_frames
    logger.info(
        "LTX t2v: generating from prompt=%r steps=%s frames=%s ...",
        text, settings.AI_LTX_NUM_INFERENCE_STEPS, num_frames or "default",
    )
    result = _run_ltx(pipe, kwargs)
    frames = result.frames[0]
    return frames, kwargs["width"], kwargs["height"]


def _resize_to_divisible_width(w: int, divisor: int = 32) -> int:
    return max(divisor, int(w) // divisor * divisor)


def _resize_to_divisible_height(h: int, divisor: int = 32) -> int:
    return max(divisor, int(h) // divisor * divisor)


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


def _generate_with_ltx(image, prompt: str, num_frames: int = None):
    """Run LTX-Video on ``image`` (+ optional ``prompt``); return ``(frames, w, h)``.

    LTX is an image-conditioned DiT video model.  ``prompt`` is optional -- when
    empty ``settings.AI_LTX_PROMPT`` is used.  ``num_frames`` is optional; when 0
    or None LTX uses its own default frame count.  Output frames are returned at
    LTX's native (VAE-divisible) resolution.
        """
    pipe = _get_pipeline("ltx")
    image = _resize_to_divisible(image, max_edge=1216, divisor=32)
    text = prompt or settings.AI_LTX_PROMPT
    _set_ltx_guidance(pipe, settings.AI_LTX_GUIDANCE_SCALE)
    kwargs = dict(
        image=image,
        prompt=text,
        num_inference_steps=settings.AI_LTX_NUM_INFERENCE_STEPS,
        generator=settings.generator(),
    )
    if num_frames:
        kwargs["num_frames"] = num_frames
    logger.info(
        "LTX: animating image (%sx%s) with prompt=%r steps=%s frames=%s ...",
        image.width,
        image.height,
        text,
        settings.AI_LTX_NUM_INFERENCE_STEPS,
        num_frames or "default",
    )
    result = _run_ltx(pipe, kwargs)
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
    model = (model or settings.AI_MODEL).lower()
    image = _pil_from_bytes(image_bytes)

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

    Prefers the real LTX-2 engine (``ltx_pipelines``/``ltx-core``) when installed
    and a CUDA GPU is present.  Otherwise falls back to the **diffusers**
    ``Lightricks/LTX-Video`` text-to-video pipeline, which needs only torch +
    diffusers (already deployed on the GPU container) -- so text-to-video works
    even on images/containers that never installed the LTX-2 python stack.

    Returns the path to the generated MP4 file.
    """
    num_frames = compute_t2v_frames(duration_seconds, int(fps))

    # Prefer the real LTX-2 engine when its packages + GPU are available
    # (skipped when AI_MODEL is wan or hunyuan, which use dedicated paths below).
    try:
        from app.services import ltx2_engine

        if ltx2_engine.ltx2_available() and settings.AI_MODEL not in ("wan", "hunyuan"):
            logger.info("Using LTX-2 DistilledPipeline for text-to-video ...")
            engine = ltx2_engine.LTX2Engine.from_settings()
            nf = ltx2_engine.compute_num_frames(duration_seconds, int(fps))
            out = engine.generate_text_to_video(
                prompt=prompt,
                output_path=str(output_path),
                width=width,
                height=height,
                duration_seconds=duration_seconds,
                num_frames=nf,
                frame_rate=fps,
                seed=seed,
            )
            return Path(out)
    except ImportError:
        pass  # ltx2 stack not installed -> fall through to diffusers
    except Exception:
        logger.warning(
            "LTX-2 text-to-video unavailable; falling back to diffusers LTX.",
            exc_info=True,
        )

    # Wan2.1 (diffusers Wan2.1 pipeline via DiffusionPipeline) -- closest LOCAL
    # model to Google Veo on a 16 GB GPU (Wan2.1-1.3B, native 720p, strong motion
    # coherence).  Enable with
    #   AI_MODEL=wan  +  AI_LTX_MODEL_ID=Wan-AI/Wan2.1-T2V-1.3B-Diffusers
    # (gated weights + diffusers >= 0.33, which ships WanPipeline).  Any failure is logged and the job is
    # marked FAILED (no silent LTX fallback) so Wan output stays attributable.
    if settings.AI_MODEL == "wan":
        try:
            logger.info("Using Wan2.1 T2V for text-to-video ...")
            frames, w, h = _generate_text_with_wan(prompt, num_frames, width, height)
            return write_frames_video(frames, output_path, fps=fps, width=w, height=h)
        except Exception:
            # A selected "wan" job must be attributable to Wan2.1: NEVER silently
            # fall back to LTX. Masking a Wan failure as an LTX clip makes it
            # impossible to tell which model produced a bad result. Re-raise so the
            # worker marks the job FAILED with the Wan traceback in job.error
            # (visible via GET /api/video/status/{job_id}).
            logger.error("Wan2.1 generation failed; marking job as failed.", exc_info=True)
            raise

    # HunyuanVideo (diffusers HunyuanVideoPipeline, 16 GB-friendly with CPU
    # offload + VAE tiling). Enable with  AI_MODEL=hunyuan
    #   +  AI_HUNYUAN_MODEL_ID=hunyuanvideo-community/HunyuanVideo
    # Any failure is logged and the job is marked FAILED (no silent fallback).
    if settings.AI_MODEL == "hunyuan":
        if not ai_available():
            raise RuntimeError(
                "diffusers+torch are not installed; cannot run HunyuanVideo"
            )
        logger.info("Using HunyuanVideo for text-to-video ...")
        try:
            frames, w, h = _generate_text_with_hunyuan(prompt, num_frames, width, height)
            return write_frames_video(frames, output_path, fps=fps, width=w, height=h)
        except Exception:
            logger.error("HunyuanVideo generation failed; marking job as failed.", exc_info=True)
            raise

    # diffusers path (no ltx-core/ltx-pipelines required).
    if not ai_available():
        raise RuntimeError(
            "No text-to-video backend available: neither LTX-2 (ltx_pipelines) "
            "nor diffusers+torch are installed."
        )
    frames, w, h = _generate_text_with_ltx(prompt, num_frames, width, height)
    return write_frames_video(frames, output_path, fps=fps, width=w, height=h)
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


def generate_image_sequence_video(
    images, output_path, prompt="", fps=None, transition_duration=None
):
    """Animate every uploaded photo with LTX image-to-video and crossfade the
    resulting clips into one continuous AI property tour.

    Each photo is treated as its own living shot (like OpenArt image-to-video),
    so every room comes alive with AI-generated camera motion.  Clips are
    normalised to the configured canvas and stitched together.
    """
    if not images:
        raise ValueError("At least one image is required")

    canvas_w, canvas_h = settings.SLIDESHOW_WIDTH, settings.SLIDESHOW_HEIGHT
    fps = fps or settings.AI_LTX_FPS
    if transition_duration is None:
        transition_duration = settings.SLIDESHOW_TRANSITION_DURATION
    num_frames = settings.AI_LTX_NUM_FRAMES or None

    clips = []
    for name, data in images:
        image = _pil_from_bytes(data)
        logger.info("AI tour: animating photo '%s' ...", name)
        frames, _, _ = _generate_with_ltx(image, prompt=prompt, num_frames=num_frames)
        clips.append(frames)

    frames_out = _stitch_clips(clips, canvas_w, canvas_h, transition_duration, fps)
    logger.info(
        "AI tour: %d shot(s) -> %d frames, %dx%d, %s",
        len(clips), len(frames_out), canvas_w, canvas_h, output_path,
    )
    return write_frames_video(
        frames_out, output_path, fps=fps, width=canvas_w, height=canvas_h
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
