"""AI image-to-video generation using HuggingFace Stable Video Diffusion.

Brings a single image to life by synthesizing a short motion clip with the
Stable Video Diffusion (SVD) model — the same class of model services like
OpenArt use for image-to-video. This module is GPU-friendly (CUDA) and is
designed to run on a machine with a decent NVIDIA GPU.

Because torch/diffusers are heavy, they are imported lazily inside the
functions. If they are not installed (or no GPU is present), the caller can
detect availability via :func:`ai_available` and fall back to a simpler
generator.
"""
import io
import logging
import threading
from fractions import Fraction
from pathlib import Path

from app.core.config import settings

logger = logging.getLogger("aivideoworker.video_generator")

# The heavy pipeline is loaded once and cached for subsequent jobs.
_pipeline = None
_pipeline_lock = threading.Lock()


def ai_available() -> bool:
    """True when torch + diffusers + Pillow are importable (i.e. SVD can run)."""
    try:
        import diffusers  # noqa: F401
        import torch  # noqa: F401
        from PIL import Image  # noqa: F401
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


def _get_pipeline():
    """Load (once) and return the cached SVD pipeline."""
    global _pipeline
    if _pipeline is not None:
        return _pipeline

    with _pipeline_lock:
        if _pipeline is not None:
            return _pipeline

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
        else:
            pipe.to("cpu")

        _pipeline = pipe
        logger.info("SVD pipeline ready on %s", settings.AI_DEVICE)
        return _pipeline


def generate_image_to_video(image_bytes: bytes, output_path: Path) -> Path:
    """Generate an MP4 that brings ``image_bytes`` to life.

    Returns the path to the written MP4. Raises on failure.
    """
    from PIL import Image

    pipe = _get_pipeline()

    image = Image.open(io.BytesIO(image_bytes)).convert("RGB")
    image = _resize_for_svd(image)

    logger.info(
        "Generating %s frames at %s fps from image (%sx%s) ...",
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

    return write_frames_webm(
        frames, output_path, fps=settings.AI_FPS, width=image.width, height=image.height
    )


def write_frames_webm(
    frames, output_path: Path, fps: int, width: int, height: int
) -> Path:
    """Write a sequence of frames to a WebM (VP9) container using PyAV.

    Accepts a list of PIL Images or BGR numpy arrays (frame BGR arrays are
    converted to RGB). WebM/VP9 is broadly playable in media players, unlike
    the MPEG-4 (mp4v) files produced by OpenCV's writer. Returns the path.
    """
    import av
    import numpy as np
    from PIL import Image

    container = av.open(str(output_path), mode="w", format="webm")
    stream = container.add_stream("libvpx-vp9", rate=Fraction(fps, 1))
    stream.width = width
    stream.height = height
    stream.pix_fmt = "yuv420p"

    try:
        for frame in frames:
            if isinstance(frame, np.ndarray):
                pil_frame = Image.fromarray(cv2.cvtColor(frame, cv2.COLOR_BGR2RGB))
            else:
                pil_frame = frame
            video_frame = av.VideoFrame.from_image(pil_frame)
            for packet in stream.encode(video_frame):
                container.mux(packet)
        # Flush the encoder.
        for packet in stream.encode():
            container.mux(packet)
    finally:
        container.close()

    return output_path