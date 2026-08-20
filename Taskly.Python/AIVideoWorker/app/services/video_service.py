"""Video-generation service.

This layer abstracts all video-creation logic from the FastAPI router.  A video
is created either by animating a single image with the configured AI model
(LTX-Video by default, Stable Video Diffusion if AI_MODEL=svd) when a GPU
+ the model weights are available, or by assembling the uploaded images into
a Ken Burns + cross-dissolve **property tour** -- the mode used for Airbnb-style
viewing videos.  Output is encoded to MP4 (H.264) by default so it plays in
every browser and on every sharing platform.

Architecture:

    HTTP Request
        ->
    video.py (router)
        ->
    VideoService
         ->  image -> AI anim (LTX/SVD)   OR   Ken Burns property tour
        ->  PyAV encoder (H.264 / VP9)
"""
import logging
import uuid
from datetime import datetime
from enum import Enum
from pathlib import Path
from threading import Thread
from typing import List, Optional, Tuple

from app.core.config import settings
from app.services import video_generator, slideshow

logger = logging.getLogger("aivideoworker.video_service")

# Frames per second used when writing the output video.
FPS = 24


class JobStatus(str, Enum):
    """Possible lifecycle states for a video-generation job."""
    QUEUED = "queued"
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"


class Job:
    """In-memory representation of a single video-generation job."""

    def __init__(
        self,
        prompt: str,
        duration: float,
        images: Optional[List[Tuple[str, bytes]]] = None,
        image_duration: Optional[float] = None,
        transition_duration: Optional[float] = None,
        kind: str = "images",
        width: Optional[int] = None,
        height: Optional[int] = None,
        fps: Optional[int] = None,
        seed: Optional[int] = None,
    ):
        self.job_id: str = str(uuid.uuid4())
        self.prompt: str = prompt
        self.duration: float = duration
        self.kind: str = kind
        self.images: List[Tuple[str, bytes]] = images or []
        # Optional overrides for the property-tour slideshow.
        self.image_duration: Optional[float] = image_duration
        self.transition_duration: Optional[float] = transition_duration
        # Text-to-video generation parameters (used when kind == "text_video").
        self.width: int = width or settings.LTX2_WIDTH
        self.height: int = height or settings.LTX2_HEIGHT
        self.fps: int = fps or settings.LTX2_FPS
        self.seed: int = seed if seed is not None else settings.LTX2_SEED
        self.status: str = JobStatus.QUEUED.value
        self.progress: int = 0
        self.video_filename: str = None
        self.error: str = None
        self.created_at: datetime = datetime.now()

    def to_dict(self) -> dict:
        result = {
            "jobId": self.job_id,
            "status": self.status,
            "progress": self.progress,
        }
        if self.video_filename:
            result["videoUrl"] = f"/api/video/download/{self.job_id}"
        if self.error:
            result["error"] = self.error
        return result


class VideoService:
    """Service that manages video-generation jobs.

    Uses an in-memory dictionary for job storage (no database yet).
    Background workers run in daemon threads.
    """

    def __init__(self):
        self.jobs: dict = {}

    def create_job(
        self,
        prompt: str,
        duration: float,
        images: Optional[List[Tuple[str, bytes]]] = None,
        image_duration: Optional[float] = None,
        transition_duration: Optional[float] = None,
        kind: str = "images",
        width: Optional[int] = None,
        height: Optional[int] = None,
        fps: Optional[int] = None,
        seed: Optional[int] = None,
    ) -> Job:
        """Create a new job and start background processing."""
        job = Job(
            prompt=prompt,
            duration=duration,
            images=images,
            image_duration=image_duration,
            transition_duration=transition_duration,
            kind=kind,
            width=width,
            height=height,
            fps=fps,
            seed=seed,
        )
        self.jobs[job.job_id] = job
        thread = Thread(target=self._process_job, args=(job.job_id,), daemon=True)
        thread.start()
        return job

    def get_job(self, job_id: str) -> Job:
        """Retrieve a job by its ID (None if not found)."""
        return self.jobs.get(job_id)

    def _process_job(self, job_id: str) -> None:
        """Background worker entry point."""
        job = self.jobs.get(job_id)
        if job is None:
            return
        try:
            logger.info("Starting job %s (%d image(s))", job_id, len(job.images))
            job.status = JobStatus.PROCESSING.value
            job.progress = 25

            if job.kind == "text_video":
                video_path = self._generate_video_from_text(job)
            else:
                video_path = self._generate_video_from_images(job)
            job.progress = 75
            job.video_filename = video_path.name
            job.progress = 100
            job.status = JobStatus.COMPLETED.value
            logger.info("Job %s completed -> %s", job_id, video_path)
        except Exception as exc:
            job.status = JobStatus.FAILED.value
            job.error = str(exc) if str(exc) else "Video generation failed"
            job.progress = 0
            logger.exception("Job %s failed: %s", job_id, exc)

    def _resolve_duration(self, job: Job) -> float:
        """Total video duration.

        If ``image_duration`` is set, each photo is shown for that many seconds
        (plus the cross-dissolve overlap) and the total is derived from it --
        handy when you want every room to linger for the same amount of time.
        Otherwise the explicitly requested ``job.duration`` is used.
        """
        n = len(job.images)
        if job.image_duration and job.image_duration > 0 and n:
            trans = job.transition_duration or settings.SLIDESHOW_TRANSITION_DURATION
            return job.image_duration * n + trans * (n - 1)
        return job.duration

    def _generate_video_from_text(self, job: Job) -> Path:
        """Generate a video purely from a text prompt using LTX-2 (text-to-video)."""
        if not job.prompt or not job.prompt.strip():
            raise ValueError("No text prompt provided")
        video_dir = settings.generated_video_path
        video_dir.mkdir(parents=True, exist_ok=True)
        video_path = video_dir / f"{job.job_id}.{settings.VIDEO_FORMAT}"
        logger.info(
            "Generating LTX-2 text-to-video: prompt=%r duration=%ss %dx%d@%dfps",
            job.prompt, job.duration, job.width, job.height, job.fps,
        )
        return video_generator.generate_text_to_video(
            prompt=job.prompt,
            output_path=video_path,
            duration_seconds=int(job.duration),
            width=job.width,
            height=job.height,
            fps=job.fps,
            seed=job.seed,
        )

    def _generate_video_from_images(self, job: Job) -> Path:
        """Generate the output video from the job's images.

        * Exactly one image + AI available -> animate it (Stable Video Diffusion).
        * Otherwise (>=1 image, with or without a GPU) -> a Ken Burns +
          cross-dissolve property tour (see :mod:`app.services.slideshow`).
        """
        if not job.images:
            raise ValueError("No images provided")

        video_dir = settings.generated_video_path
        video_dir.mkdir(parents=True, exist_ok=True)
        video_path = video_dir / f"{job.job_id}.{settings.VIDEO_FORMAT}"

        # Single image + AI -> synthesize motion with the configured model
        # (LTX-Video by default; SVD when AI_MODEL=svd).
        if len(job.images) == 1 and video_generator.ai_available():
            name, data = job.images[0]
            logger.info(
                "Animating image '%s' with %s ...", name, settings.AI_MODEL.upper()
            )
            return video_generator.generate_image_to_video(
                data, video_path, prompt=job.prompt, model=settings.AI_MODEL
            )

        # Multiple photos + GPU + TOUR_STYLE=ai -> animate each photo with the AI
        # model (LTX image-to-video) and crossfade the shots into a living tour.
        if (
            len(job.images) > 1
            and video_generator.ai_available()
            and settings.TOUR_STYLE == "ai"
        ):
            try:
                logger.info(
                    "Building AI property tour from %d photo(s) with %s ...",
                    len(job.images), settings.AI_MODEL.upper(),
                )
                return video_generator.generate_image_sequence_video(
                    job.images, video_path, prompt=job.prompt
                )
            except Exception:
                logger.warning(
                    "AI property tour failed; falling back to Ken Burns slideshow.",
                    exc_info=True,
                )

        # Otherwise a Ken Burns slideshow is produced. This covers: no GPU / no model,
        # TOUR_STYLE=slideshow, or an AI-tour failure already logged above.
        logger.info(
            "Using Ken Burns slideshow for %d photo(s) "
            "(ai_available=%s, tour_style=%s).",
            len(job.images), video_generator.ai_available(), settings.TOUR_STYLE,
        )
        duration = self._resolve_duration(job)
        return slideshow.generate_slideshow(
            images=job.images,
            output_path=video_path,
            duration=duration,
            fps=FPS,
            transition_duration=job.transition_duration,
        )
