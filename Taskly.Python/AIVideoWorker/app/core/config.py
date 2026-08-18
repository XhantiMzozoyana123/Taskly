"""
Application configuration loaded from environment variables.
Uses python-dotenv to read from the .env file.
"""
import os
from pathlib import Path

from dotenv import load_dotenv

# Load .env from the project root
PROJECT_ROOT = Path(__file__).resolve().parent.parent.parent
load_dotenv(PROJECT_ROOT / ".env")


class Settings:
    """Strongly-typed application settings backed by environment variables."""

    # FastAPI / server settings
    HOST: str = os.getenv("HOST", "127.0.0.1")
    PORT: int = int(os.getenv("PORT", "8000"))

    # Generated-video storage location (relative to the project root)
    GENERATED_VIDEO_DIRECTORY: str = os.getenv(
        "GENERATED_VIDEO_DIRECTORY", "generated"
    )

    # --- AI image-to-video settings ---
    # "cuda" uses the GPU; "cpu" runs (slowly) without one.
    AI_DEVICE: str = os.getenv("AI_DEVICE", "cuda")
    # Stable Video Diffusion (used when AI_MODEL=svd).
    AI_MODEL_ID: str = os.getenv(
        "AI_MODEL_ID", "stabilityai/stable-video-diffusion-img2vid"
    )
    AI_USE_FP16: bool = os.getenv("AI_USE_FP16", "true").lower() in ("1", "true", "yes")
    # Stream model layers between CPU/GPU to save VRAM (slower).
    # DISABLED by default: with SVD + fp16, CPU-offloading causes a device
    # mismatch ("Input type torch.cuda.HalfTensor and weight type torch.HalfTensor
    # should be the same") because the input lands on CUDA while weights are off-
    # loaded to CPU.  The RTX A4000 (16 GB) has enough VRAM to hold the full SVD
    # pipeline in fp16, so offloading is unnecessary for this hardware.
    AI_CPU_OFFLOAD: bool = os.getenv("AI_CPU_OFFLOAD", "false").lower() in (
        "1", "true", "yes",
    )
    AI_NUM_FRAMES: int = int(os.getenv("AI_NUM_FRAMES", "14"))
    AI_FPS: int = int(os.getenv("AI_FPS", "7"))
    AI_DECODE_CHUNK_SIZE: int = int(os.getenv("AI_DECODE_CHUNK_SIZE", "8"))
    AI_MOTION_BUCKET_ID: int = int(os.getenv("AI_MOTION_BUCKET_ID", "127"))
    AI_NOISE_AUG_STRENGTH: float = float(os.getenv("AI_NOISE_AUG_STRENGTH", "0.02"))
    AI_MAX_EDGE: int = int(os.getenv("AI_MAX_EDGE", "1024"))
    AI_SEED: int = int(os.getenv("AI_SEED", "42"))

    # --- AI image-to-video model selection ---
    # Which diffusion model animates a single image when a GPU is available.
    #   "ltx" -> Lightricks LTX-Video (fast, OpenArt-style motion) [default]
    #   "svd" -> Stable Video Diffusion
    AI_MODEL: str = os.getenv("AI_MODEL", "ltx")
    # LTX-Video model id (Lightricks/LTX-Video, ~2B params, bfloat16, fits 8GB+ GPUs).
    AI_LTX_MODEL_ID: str = os.getenv("AI_LTX_MODEL_ID", "Lightricks/LTX-Video")
    AI_LTX_NUM_INFERENCE_STEPS: int = int(os.getenv("AI_LTX_NUM_INFERENCE_STEPS", "30"))
    AI_LTX_FPS: int = int(os.getenv("AI_LTX_FPS", "24"))
    # Default text conditioning for LTX (used when a job has no prompt).
    AI_LTX_PROMPT: str = os.getenv(
        "AI_LTX_PROMPT",
        "real estate property tour, smooth camera motion, bright natural lighting",
    )

    # --- Generated-video output (slideshow / property tour) ---
    # Container + codec used for the fallback (multi-image / no-GPU) path.
    # "mp4" -> H.264 (broadly compatible, great for Airbnb/OpenArt sharing).
    # "webm" -> VP9 (still supported for backward compatibility).
    VIDEO_FORMAT: str = os.getenv("VIDEO_FORMAT", "mp4")
    # Per-image Ken Burns zoom range (1.0 = full frame, 0.7 = 1.43x zoom-in).
    SLIDESHOW_WIDTH: int = int(os.getenv("SLIDESHOW_WIDTH", "1280"))
    SLIDESHOW_HEIGHT: int = int(os.getenv("SLIDESHOW_HEIGHT", "720"))
    SLIDESHOW_TRANSITION_DURATION: float = float(
        os.getenv("SLIDESHOW_TRANSITION_DURATION", "1.0")
    )
    SLIDESHOW_ZOOM_MIN: float = float(os.getenv("SLIDESHOW_ZOOM_MIN", "0.7"))
    SLIDESHOW_ZOOM_MAX: float = float(os.getenv("SLIDESHOW_ZOOM_MAX", "1.15"))
    SLIDESHOW_SEED: int = int(os.getenv("SLIDESHOW_SEED", "42"))

    @property
    def generated_video_path(self) -> Path:
        """Absolute path to the directory where generated videos are stored."""
        return PROJECT_ROOT / self.GENERATED_VIDEO_DIRECTORY

    def generator(self):
        """A seeded torch Generator for reproducible SVD/LTX output.

        Imported lazily so non-AI imports do not require torch.
        """
        import torch

        return torch.Generator(device=self.AI_DEVICE).manual_seed(self.AI_SEED)


settings = Settings()  # singleton used throughout the application
