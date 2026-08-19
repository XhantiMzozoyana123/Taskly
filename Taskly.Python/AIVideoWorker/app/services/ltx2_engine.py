"""LTX-2 image-to-video / text-to-video engine for AIVideoWorker.

Faithful adapter of **LTX Desktop**'s local-generation engine (Apache-2.0). It
wraps Lightricks' ``ltx_pipelines.distilled.DistilledPipeline`` (LTX 2.5 / 2.3
Fast distilled) so AIVideoWorker can generate locally on a CUDA GPU using the
same LTX-2 technology as LTX Desktop.

To run (on the GPU host):
  * Python 3.12+
  * CUDA GPU >= 16 GB VRAM (weights are streamed to fit VRAM)
  * Gated LTX-2 checkpoints: set ``HF_TOKEN`` and accept model licenses
  * Install ``requirements-ltx2.txt`` / build ``Dockerfile.ltx2``

All imports are lazy, so the API and the Ken Burns fallback still boot on a box
without the LTX-2 stack.  Vendored helpers mirror LTX-Desktop's
``backend/services/{fast_video_pipeline,ltx_pipeline_common}.py`` (Apache-2.0).
"""
from __future__ import annotations

import logging
import threading
from pathlib import Path

from app.core.config import settings

logger = logging.getLogger("aivideoworker.ltx2")


def ltx2_available() -> bool:
    """True when the LTX-2 packages import and a CUDA GPU is reachable."""
    try:
        import torch  # noqa: F401
        import ltx_pipelines  # noqa: F401

        return bool(torch.cuda.is_available())
    except Exception:
        return False


def snap_to_frame_grid(n: int, *, floor: int = 9) -> int:
    """Snap a frame count down to the pipeline's valid ``(n - 1) % 8 == 0`` grid."""
    return max(floor, ((max(1, n) - 1) // 8) * 8 + 1)


def compute_num_frames(duration_seconds: int, fps: int) -> int:
    """Frame count for a duration on the ``(n - 1) % 8 == 0`` grid."""
    return max(((duration_seconds * fps) // 8) * 8 + 1, 9)


def _device(device: str):
    import torch

    return torch.device(device or "cuda")


def _device_supports_fp8(device) -> bool:
    import torch

    try:
        return device.type == "cuda" and torch.cuda.is_available()
    except Exception:
        return False


def _build_model_paths(checkpoint, gemma_root, *, video_vae, audio_vae, duration_head):
    """``ModelPaths`` for split (2.5) or monolith (2.3) checkpoint layouts."""
    from ltx_pipelines.utils.model_paths import ModelPaths

    if video_vae is not None and audio_vae is not None:
        return ModelPaths.from_split(
            transformer_path=checkpoint,
            text_encoder_path=gemma_root,
            video_vae_path=video_vae,
            audio_vae_path=audio_vae,
            duration_head_path=duration_head,
        )
    return ModelPaths.from_monolith(checkpoint, gemma_root, video_vae_path=video_vae)


def _auto_tiling_config():
    from ltx_core.model.video_vae import AUTO_TILING

    return AUTO_TILING


def _offload_mode(streaming_prefetch_count, device):
    """streaming_prefetch_count is None -> resident; otherwise stream weights."""
    from ltx_pipelines.utils.types import OffloadMode

    if streaming_prefetch_count is None:
        return OffloadMode.NONE
    if device.type == "mps":
        return OffloadMode.DISK
    return OffloadMode.CPU


def _video_chunks_number(num_frames: int, tiling_config) -> int:
    from ltx_core.model.video_vae import get_video_chunks_number

    return int(get_video_chunks_number(num_frames, tiling_config))


def _encode_video(video, audio, fps, output_path, num_frames, tiling) -> None:
    from ltx_pipelines.utils.media_io import encode_video

    encode_video(
        video=video,
        fps=int(fps),
        audio=audio,
        output_path=str(output_path),
        video_chunks_number=_video_chunks_number(num_frames, tiling),
    )


class LTX2Engine:
    """A thin, faithful wrapper around ``ltx_pipelines.distilled.DistilledPipeline``.

    The pipeline is built once and reused.  Weight streaming and fp8 are handled
    by the LTX-2 stack exactly as in LTX Desktop.
    """

    _instance = None
    _instance_lock = threading.Lock()

    def __init__(
        self,
        *,
        checkpoint_path: str,
        gemma_root: str | None = None,
        upsampler_path: str | None = None,
        video_vae_path: str | None = None,
        audio_vae_path: str | None = None,
        duration_head_path: str | None = None,
        device: str = "cuda",
        streaming_prefetch_count: int | None = 4,
        loras: list[tuple[str, float]] | None = None,
    ):
        self.checkpoint_path = checkpoint_path
        self.gemma_root = gemma_root
        self.upsampler_path = upsampler_path
        self.video_vae_path = video_vae_path
        self.audio_vae_path = audio_vae_path
        self.duration_head_path = duration_head_path
        self._device = _device(device)
        self._streaming = streaming_prefetch_count
        self._loras = loras or []
        self._pipeline = None

    # -- lifecycle ----------------------------------------------------------
    def _load(self):
        if self._pipeline is not None:
            return self._pipeline

        from ltx_pipelines.distilled import DistilledPipeline
        from ltx_core.quantization.fp8_cast import build_policy as build_fp8_cast_policy

        quantization = None
        if _device_supports_fp8(self._device):
            try:
                quantization = build_fp8_cast_policy(self.checkpoint_path)
            except Exception:
                logger.warning("LTX-2 fp8 unavailable; using bf16", exc_info=True)

        self._pipeline = DistilledPipeline(
            model_paths=_build_model_paths(
                self.checkpoint_path,
                self.gemma_root,
                video_vae=self.video_vae_path,
                audio_vae=self.audio_vae_path,
                duration_head=self.duration_head_path,
            ),
            spatial_upsampler_path=self.upsampler_path,
            device=self._device,
            quantization=quantization,
            offload_mode=_offload_mode(self._streaming, self._device),
        )
        logger.info("LTX-2 DistilledPipeline ready on %s", self._device)
        return self._pipeline

    # -- generation ---------------------------------------------------------
    def _generate(
        self,
        *,
        prompt: str,
        images: list[tuple[str, int, float]],
        output_path: str,
        width: int,
        height: int,
        num_frames: int,
        frame_rate: float,
        seed: int,
    ) -> str:
        from ltx_pipelines.utils.args import ImageConditioningInput

        pipeline = self._load()
        result = pipeline(
            prompt=prompt,
            seed=seed,
            height=height,
            width=width,
            num_frames=snap_to_frame_grid(num_frames),
            frame_rate=float(frame_rate),
            images=[ImageConditioningInput(path=p, frame_idx=f, strength=s) for (p, f, s) in images],
            tiling_config=_auto_tiling_config(),
        )
        video, audio, resolved_frames, resolved_tiling = result
        _encode_video(
            video=video,
            audio=audio,
            fps=frame_rate,
            output_path=output_path,
            num_frames=resolved_frames,
            tiling=resolved_tiling,
        )
        return output_path

    def generate_image_to_video(
        self,
        image_path: str,
        prompt: str,
        output_path: str,
        *,
        width: int = 1280,
        height: int = 720,
        duration_seconds: int = 4,
        frame_rate: float = 25,
        seed: int = 0,
        strength: float = 1.0,
    ) -> str:
        """Condition the LTX-2 clip on ``image_path`` (image-to-video)."""
        return self._generate(
            prompt=prompt,
            images=[(image_path, 0, strength)],
            output_path=output_path,
            width=width,
            height=height,
            num_frames=compute_num_frames(duration_seconds, int(frame_rate)),
            frame_rate=frame_rate,
            seed=seed,
        )

    def generate_text_to_video(
        self,
        prompt: str,
        output_path: str,
        *,
        width: int = 1280,
        height: int = 720,
        duration_seconds: int = 4,
        num_frames: int = 0,
        frame_rate: float = 25,
        seed: int = 0,
    ) -> str:
        if not num_frames:
            num_frames = compute_num_frames(duration_seconds, int(frame_rate))
        return self._generate(
            prompt=prompt,
            images=[],
            output_path=output_path,
            width=width,
            height=height,
            num_frames=num_frames,
            frame_rate=frame_rate,
            seed=seed,
        )

    @classmethod
    def from_settings(cls):
        """Build or return the process-wide engine from ``settings``."""
        with cls._instance_lock:
            if cls._instance is None:
                cls._instance = cls(
                    checkpoint_path=settings.LTX2_CHECKPOINT_DIR,
                    gemma_root=settings.LTX2_GEMMA_DIR or None,
                    upsampler_path=settings.LTX2_UPSCALER_PATH or None,
                    video_vae_path=settings.LTX2_VIDEO_VAE_PATH or None,
                    audio_vae_path=settings.LTX2_AUDIO_VAE_PATH or None,
                    duration_head_path=settings.LTX2_DURATION_HEAD_PATH or None,
                    device=settings.AI_DEVICE,
                )
            return cls._instance