"""Pydantic request/response models for the video-generation API."""
from typing import Optional
from pydantic import BaseModel, Field


class VideoGenerateRequest(BaseModel):
    """Request body for POST /api/video/generate."""
    prompt: str = Field(..., description="Text describing the video to generate")
    duration: int = Field(default=5, ge=1, description="Video duration in seconds")


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