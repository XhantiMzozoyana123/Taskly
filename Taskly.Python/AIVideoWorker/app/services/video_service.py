"""Video-generation service.

This layer abstracts all video-creation logic from the FastAPI router.
A video is created by assembling the uploaded images into a playable MP4
using OpenCV: each image is shown for ``duration / len(images)`` seconds.

Architecture:

    HTTP Request
        ->
    video.py (router)
        ->
    VideoService
        ->
    Image -> MP4 assembler (OpenCV)
"""
import time
import uuid
from datetime import datetime
from enum import Enum
from pathlib import Path
from threading import Thread
from typing import List, Optional, Tuple

import cv2
import numpy as np

from app.core.config import settings
from app.services import video_generator

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
    ):
        self.job_id: str = str(uuid.uuid4())
        self.prompt: str = prompt
        self.duration: float = duration
        self.images: List[Tuple[str, bytes]] = images or []
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
    ) -> Job:
        """Create a new job and start background processing."""
        job = Job(prompt=prompt, duration=duration, images=images)
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
            print(f"INFO - Starting job {job_id}")

            job.status = JobStatus.PROCESSING.value
            job.progress = 25
            time.sleep(1)

            job.progress = 50
            time.sleep(1)

            # Assemble the uploaded images into a playable MP4.
            video_path = self._generate_video_from_images(job)
            job.progress = 75
            time.sleep(1)

            job.video_filename = video_path.name
            job.progress = 100
            job.status = JobStatus.COMPLETED.value
            print(f"INFO - Job {job_id} completed")
            print(f"INFO - Video saved to {video_path}")

        except Exception as exc:
            job.status = JobStatus.FAILED.value
            job.error = str(exc) if str(exc) else "Video generation failed"
            job.progress = 0
            print(f"ERROR - Job {job_id} failed: {exc}")

    def _generate_video_from_images(self, job: Job) -> Path:
        """Generate the output MP4 from the job's images.

        - Exactly one image + AI available  -> animate it (Stable Video Diffusion).
        - Otherwise                          -> slideshow fallback (OpenCV).
        """
        if not job.images:
            raise ValueError("No images provided")

        video_dir = settings.generated_video_path
        video_dir.mkdir(parents=True, exist_ok=True)
        video_path = video_dir / f"{job.job_id}.mp4"

        if len(job.images) == 1 and video_generator.ai_available():
            name, data = job.images[0]
            print(f"INFO - Animating image '{name}' with Stable Video Diffusion ...")
            return video_generator.generate_image_to_video(data, video_path)

        print("INFO - Using slideshow mode (needs a single image + AI model)")
        return self._generate_slideshow(job, video_path)

    def _generate_slideshow(self, job: Job, video_path: Path) -> Path:
        """Fallback: build a slideshow MP4 by showing each uploaded image for a
        slice of the requested duration (OpenCV writer, mp4v codec)."""
        # Decode each uploaded image into a BGR frame.
        frames: List[np.ndarray] = []
        for name, data in job.images:
            frame = cv2.imdecode(np.frombuffer(data, dtype=np.uint8), cv2.IMREAD_COLOR)
            if frame is None:
                raise ValueError(f"Could not decode image: {name}")
            frames.append(frame)

        # Normalize all frames to the size of the first image.
        height, width = frames[0].shape[:2]
        for i, frame in enumerate(frames):
            if frame.shape[:2] != (height, width):
                frames[i] = cv2.resize(frame, (width, height))

        # Number of times each image is repeated to fill its slice of the duration.
        frames_per_image = max(1, int(job.duration * FPS / len(frames)))

        writer = cv2.VideoWriter(
            str(video_path),
            cv2.VideoWriter_fourcc(*"mp4v"),
            FPS,
            (width, height),
        )
        if not writer.isOpened():
            raise RuntimeError("Could not open the MP4 video writer (mp4v codec)")

        try:
            for frame in frames:
                for _ in range(frames_per_image):
                    writer.write(frame)
        finally:
            writer.release()

        return video_path