# syntax=docker/dockerfile:1
#
# AIVideoWorker - FastAPI image-to-video service.
#
# Uses PyTorch's CUDA-enabled runtime image so PyTorch + CUDA libraries are
# already present (matches the VPS's RTX A4000 / CUDA 12.4 driver family).
# The SVD model is downloaded to HuggingFace's cache at first run.

FROM pytorch/pytorch:2.5.1-cuda12.4-cudnn9-runtime

ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    PIP_NO_CACHE_DIR=1 \
    PIP_DISABLE_PIP_VERSION_CHECK=1

WORKDIR /app

# Install app dependencies (torch is already in the base image, so pip treats
# the "torch" line as satisfied and will not re-download it).
COPY requirements.txt /app/requirements.txt
RUN pip install --upgrade pip \
    && pip install -r /app/requirements.txt

# Copy the application source into the image.
COPY . /app

# Port used by uvicorn (make sure it is open in the VPS firewall).
EXPOSE 8000

# Health check so the host / orchestrator can confirm liveness.
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD python -c "import urllib.request,sys; sys.exit(0) if urllib.request.urlopen('http://127.0.0.1:8000/health', timeout=4).status==200 else sys.exit(1)"

# Run without auto-reload (production). Host/port are configurable via env
# (defaults come from app.core.config, or the compose file).
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]