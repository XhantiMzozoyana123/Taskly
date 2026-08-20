"""Pydantic request/response models for the video-generation API."""
from typing import List, Optional

from pydantic import BaseModel, Field


class VideoGenerateRequest(BaseModel):
    """Legacy request body for POST /api/video/generate (form-data, unused now)."""
    prompt: str = Field(..., description="Text describing the video to generate")
    duration: int = Field(default=5, ge=1, description="Video duration in seconds")


class JsonImageInput(BaseModel):
    """A single image supplied as base64 for the JSON endpoint."""
    filename: str = Field(default="image.png", description="Original file name")
    content_type: str = Field(default="image/png", description="MIME type, e.g. image/png")
    data_base64: str = Field(..., description="Base64-encoded image bytes")


class VideoGenerateJsonRequest(BaseModel):
    """Request body for POST /api/video/generate-json (JSON + base64 images)."""
    prompt: str = Field(default="", description="Optional text describing the video")
    duration: float = Field(default=5.0, ge=1, description="Video duration in seconds")
    images: List[JsonImageInput] = Field(..., min_items=1, description="One or more images")
    image_duration: Optional[float] = Field(
        default=None, ge=0,
        description="Seconds each photo stays on screen; overrides total duration when > 0",
    )
    transition_duration: Optional[float] = Field(
        default=None, ge=0, description="Seconds of cross-dissolve between photos (0 = auto)"
    )


class TextVideoGenerateRequest(BaseModel):
    """Request body for POST /api/video/generate-text-video (text-to-video)."""

    prompt: str = Field(..., description="Text describing the video to generate")
    duration: float = Field(default=4.0, ge=1, description="Video duration in seconds")
    width: Optional[int] = Field(default=1280, ge=64, description="Video width in pixels")
    height: Optional[int] = Field(default=720, ge=64, description="Video height in pixels")
    fps: Optional[int] = Field(default=25, ge=1, le=60, description="Frames per second")
    seed: Optional[int] = Field(default=42, description="Random seed (0 = random)")


class VideoGenerateResponse(BaseModel):
    """Response returned immediately after queuing a video-generation job."""
    success: bool
    jobId: str
    status: str


class VideoStatusResponse(BaseModel):
    """Response returned by GET /api/video/status/{job_id}."""
    jobId: str
    status: str
    progress: int
    videoUrl: Optional[str] = None
    error: Optional[str] = None


class VideoErrorResponse(BaseModel):
    """Error detail returned by the API on failure."""
    detail: str
    jobId: Optional[str] = None
