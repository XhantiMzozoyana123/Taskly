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

    # --- AI image-to-video (Stable Video Diffusion) settings ---
    # "cuda" uses the GPU; "cpu" runs (slowly) without one.
    AI_DEVICE: str = os.getenv("AI_DEVICE", "cuda")
    AI_MODEL_ID: str = os.getenv(
        "AI_MODEL_ID", "stabilityai/stable-video-diffusion-img2vid"
    )
    AI_USE_FP16: bool = os.getenv("AI_USE_FP16", "true").lower() in ("1", "true", "yes")
    # Stream model layers between CPU/GPU to save VRAM (slower).
    AI_CPU_OFFLOAD: bool = os.getenv("AI_CPU_OFFLOAD", "true").lower() in (
        "1", "true", "yes",
    )
    AI_NUM_FRAMES: int = int(os.getenv("AI_NUM_FRAMES", "14"))
    AI_FPS: int = int(os.getenv("AI_FPS", "7"))
    AI_DECODE_CHUNK_SIZE: int = int(os.getenv("AI_DECODE_CHUNK_SIZE", "8"))
    AI_MOTION_BUCKET_ID: int = int(os.getenv("AI_MOTION_BUCKET_ID", "127"))
    AI_NOISE_AUG_STRENGTH: float = float(os.getenv("AI_NOISE_AUG_STRENGTH", "0.02"))
    AI_MAX_EDGE: int = int(os.getenv("AI_MAX_EDGE", "1024"))
    AI_SEED: int = int(os.getenv("AI_SEED", "42"))

    @property
    def generated_video_path(self) -> Path:
        """Absolute path to the directory where generated videos are stored."""
        return PROJECT_ROOT / self.GENERATED_VIDEO_DIRECTORY

    def generator(self):
        """A seeded torch Generator for reproducible SVD output.

        Imported lazily so non-AI imports don't require torch.
        """
        import torch

        return torch.Generator(device=self.AI_DEVICE).manual_seed(self.AI_SEED)


settings = Settings()  # singleton used throughout the application