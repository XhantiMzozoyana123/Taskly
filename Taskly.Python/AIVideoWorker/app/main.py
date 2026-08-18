"""Entry point for the AIVideoWorker FastAPI application."""
from fastapi import FastAPI

from app.api.video import router as video_router

app = FastAPI(title="AI Video Worker", version="1.0.0")
app.include_router(video_router)


@app.get("/health")
def health():
    """Health-check endpoint used by Taskly.Form and monitoring tools."""
    return {"status": "healthy", "service": "AIVideoWorker"}