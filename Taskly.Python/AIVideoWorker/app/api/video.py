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
    TextVideoGenerateRequest,
    VideoGenerateJsonRequest,
    VideoGenerateResponse,
)
from app.services.video_service import VideoService

video_service = VideoService()
router = APIRouter(prefix="/api/video", tags=["video"])


def _decode_base64_image(value: str, filename: str) -> bytes:
    """Decode a base64 string into raw image bytes.

    Tolerates a leading ``data:image/...;base64,`` data-URI prefix, URL-safe
    base64 (``-``/``_`` instead of ``+``//``) and missing ``=`` padding.  The
    result is validated by actually decoding it, so a non-image payload is
    rejected here with a clear 400 instead of being silently accepted and later
    dying inside the AI tour (which used to surface only as a Ken Burns
    fallback with a swallowed traceback).
    """
    value = value.strip()
    if value.startswith("data:"):
        _, _, value = value.partition(",")

    # Drop any embedded whitespace / newlines (some clients wrap long b64).
    value = "".join(value.split())
    # URL-safe base64 uses '-'/'_' where the standard alphabet uses '+'/'/'.
    # If left untouched, base64.b64decode(validate=True) raises and the lenient
    # (validate=False) form silently drops those chars and shifts byte
    # alignment -- producing bytes Pillow cannot identify even though OpenCV
    # can still recover an image.  Normalise to the standard alphabet so the
    # decoded payload is byte-identical to the original image.
    value = value.replace("-", "+").replace("_", "/")
    # Restore any missing '=' padding (b64decode mandates len % 4 == 0).
    value += "=" * (-len(value) % 4)

    try:
        data = base64.b64decode(value, validate=True)
    except Exception as exc:
        raise HTTPException(
            status_code=400, detail=f"Invalid base64 data for image '{filename}': {exc}"
        )
    if not data:
        raise HTTPException(
            status_code=400, detail=f"Image '{filename}' decoded to empty bytes"
        )

    # Fail fast: make sure the payload really is an image we can decode.
    try:
        from app.services.video_generator import _pil_from_bytes

        _pil_from_bytes(data)
    except Exception:
        raise HTTPException(
            status_code=400,
            detail=(
                f"image '{filename}' is not a valid image "
                "(could not be decoded by Pillow or OpenCV)"
            ),
        )
    return data


def _media_type_for(filename: str) -> str:
    """Map a video filename's extension to a MIME media type."""
    ext = filename.rsplit(".", 1)[-1].lower()
    return {
        "mp4": "video/mp4",
        "m4v": "video/mp4",
        "mov": "video/quicktime",
        "webm": "video/webm",
        "mkv": "video/x-matroska",
    }.get(ext, "application/octet-stream")


@router.post("/generate", response_model=VideoGenerateResponse, status_code=200)
async def generate_video(
    images: List[UploadFile] = File(...),
    prompt: str = Form("", description="Optional text describing the video"),
    duration: float = Form(default=5.0, description="Video duration in seconds"),
    imageDuration: float = Form(
        default=0.0,
        description="Seconds each photo stays on screen; overrides total duration when > 0",
    ),
    transitionDuration: float = Form(
        default=0.0,
        description="Seconds of cross-dissolve between photos (0 = use app default)",
    ),
):
    """Generate a video from the uploaded images (multipart form).

    Upload one or more property photos and receive a ``jobId``.  When several
    images are supplied (or no GPU is present) a Ken Burns + cross-dissolve
    property tour is produced; a single image with the AI model available is
    animated with the configured AI model (LTX-Video by default, or SVD) instead.  Poll ``GET /status/{jobId}``
    and then download the result with ``GET /download/{jobId}``.
    """
    image_payload: List[tuple] = []
    for upload in images:
        data = await upload.read()
        if not data:
            continue
        image_payload.append((upload.filename or "image", data))

    if not image_payload:
        raise HTTPException(status_code=400, detail="At least one valid image is required")

    job = video_service.create_job(
        prompt=prompt,
        duration=duration,
        images=image_payload,
        image_duration=imageDuration or None,
        transition_duration=transitionDuration or None,
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
        prompt=request.prompt,
        duration=request.duration,
        images=image_payload,
        image_duration=request.image_duration or None,
        transition_duration=request.transition_duration or None,
    )
    print(
        f"INFO - Video generation requested (json): prompt={request.prompt!r}, "
        f"duration={request.duration}, images={len(image_payload)}"
    )
    print(f"INFO - Created job {job.job_id}")
    return VideoGenerateResponse(success=True, jobId=job.job_id, status=job.status)


@router.post(
    "/generate-text-video", response_model=VideoGenerateResponse, status_code=200
)
def generate_text_video(request: TextVideoGenerateRequest):
    """Generate a video from a text prompt.

    The job is queued and its backend is selected by the background worker:
    it uses the real LTX-2 DistilledPipeline when ``ltx_pipelines`` is
    installed and a CUDA GPU is present, otherwise falls back to the diffusers
    ``Lightricks/LTX-Video`` text-to-video pipeline (torch + diffusers, already
    deployed).  Poll ``GET /api/video/status/{job_id}`` and download with
    ``GET /api/video/download/{job_id}`` once ``status`` is ``"completed"``.
    """
    # Fail fast only when no usable backend exists at all; otherwise let the
    # background worker choose LTX-2 vs the diffusers fallback.
    from app.services import video_generator

    ltx2_ok = False
    try:
        from app.services import ltx2_engine

        ltx2_ok = bool(getattr(settings, "LTX2_ENABLED", False)) and ltx2_engine.ltx2_available()
    except ImportError:
        ltx2_ok = False

    if not ltx2_ok and not video_generator.ai_available():
        raise HTTPException(
            status_code=503,
            detail=(
                "No text-to-video backend available: LTX-2 engine requires "
                "ltx_pipelines/ltx-core + CUDA GPU, and the diffusers fallback "
                "(torch/diffusers) is not installed."
            ),
        )

    job = video_service.create_job(
        prompt=request.prompt,
        duration=request.duration,
        kind="text_video",
        width=request.width,
        height=request.height,
        fps=request.fps,
        seed=request.seed,
    )
    print(
        f"INFO - Text-to-video requested: prompt={request.prompt!r}, "
        f"duration={request.duration}, res={request.width or 1280}x{request.height or 720}"
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
    """Download the generated video for a completed job."""
    job = video_service.get_job(job_id)
    if job is None:
        raise HTTPException(status_code=404, detail=f"Job {job_id} not found")
    if job.status != "completed" or not job.video_filename:
        raise HTTPException(status_code=404, detail="Video not yet generated or job not completed")

    video_path = settings.generated_video_path / job.video_filename
    if not video_path.exists():
        raise HTTPException(status_code=404, detail="Video file not found on server")

    return FileResponse(
        path=str(video_path),
        media_type=_media_type_for(job.video_filename),
        filename=job.video_filename,
    )
