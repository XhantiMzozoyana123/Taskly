"""FastAPI router for video-generation endpoints.

Prefix: /api/video

Endpoints:
    POST /api/video/generate        (multipart: images + prompt + duration)
    POST /api/video/generate-json   (JSON body + base64 images)
    GET  /api/video/status/{job_id}
    GET  /api/video/download/{job_id}
"""
import base64
from typing import List

from fastapi import APIRouter, File, Form, HTTPException, UploadFile
from fastapi.responses import FileResponse

from app.core.config import settings
from app.models.video_models import (
    VideoGenerateJsonRequest,
    VideoGenerateResponse,
)
from app.services.video_service import VideoService

video_service = VideoService()
router = APIRouter(prefix="/api/video", tags=["video"])


def _decode_base64_image(value: str, filename: str) -> bytes:
    """Decode a base64 string into image bytes, tolerating a data-URI prefix."""
    value = value.strip()
    if value.startswith("data:"):
        _, _, value = value.partition(",")
    try:
        data = base64.b64decode(value, validate=False)
    except Exception as exc:
        raise HTTPException(
            status_code=400, detail=f"Invalid base64 data for image '{filename}': {exc}"
        )
    if not data:
        raise HTTPException(
            status_code=400, detail=f"Image '{filename}' decoded to empty bytes"
        )
    return data


@router.post("/generate", response_model=VideoGenerateResponse, status_code=200)
async def generate_video(
    images: List[UploadFile] = File(...),
    prompt: str = Form("", description="Optional text describing the video"),
    duration: float = Form(default=5.0, description="Video duration in seconds"),
):
    """Generate a video from the uploaded images (multipart form)."""
    image_payload: List[tuple] = []
    for upload in images:
        data = await upload.read()
        if not data:
            continue
        image_payload.append((upload.filename or "image", data))

    if not image_payload:
        raise HTTPException(status_code=400, detail="At least one valid image is required")

    job = video_service.create_job(
        prompt=prompt, duration=duration, images=image_payload
    )
    print(
        f"INFO - Video generation requested: prompt={prompt!r}, "
        f"duration={duration}, images={len(image_payload)}"
    )
    print(f"INFO - Created job {job.job_id}")
    return VideoGenerateResponse(success=True, jobId=job.job_id, status=job.status)


@router.post(
    "/generate-json", response_model=VideoGenerateResponse, status_code=200
)
def generate_video_json(request: VideoGenerateJsonRequest):
    """Generate a video from base64 images supplied as a JSON body."""
    image_payload: List[tuple] = []
    for img in request.images:
        data = _decode_base64_image(img.data_base64, img.filename)
        image_payload.append((img.filename or "image", data))

    job = video_service.create_job(
        prompt=request.prompt, duration=request.duration, images=image_payload
    )
    print(
        f"INFO - Video generation requested (json): prompt={request.prompt!r}, "
        f"duration={request.duration}, images={len(image_payload)}"
    )
    print(f"INFO - Created job {job.job_id}")
    return VideoGenerateResponse(success=True, jobId=job.job_id, status=job.status)


@router.get("/status/{job_id}")
def get_video_status(job_id: str):
    """Retrieve the current status of a video-generation job."""
    job = video_service.get_job(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail=f"Job {job_id} not found")

    result = {"jobId": job.job_id, "status": job.status, "progress": job.progress}
    if job.video_filename:
        result["videoUrl"] = f"/api/video/download/{job.job_id}"
    if job.error:
        result["error"] = job.error
    return result


@router.get("/download/{job_id}")
def download_video(job_id: str):
    """Download the generated WebM for a completed job."""
    job = video_service.get_job(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail=f"Job {job_id} not found")
    if job.status != "completed" or not job.video_filename:
        raise HTTPException(status_code=404, detail="Video not yet generated or job not completed")

    video_path = settings.generated_video_path / job.video_filename
    if not video_path.exists():
        raise HTTPException(status_code=404, detail="Video file not found on server")

    return FileResponse(
        path=str(video_path), media_type="video/webm", filename=job.video_filename
    )