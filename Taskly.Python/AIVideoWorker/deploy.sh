#!/bin/bash
# AIVideoWorker LTX-2 Deployment Script
set -e

echo "🚀 Setting up AIVideoWorker LTX-2 deployment..."

# Create .env for LTX-2 settings
cat > .env <<'EOF'
VIDEO_OUTPUT_DIR=/app/output
IMAGE_DIR=/app/input

# LTX-Video 2.x Engine Settings
LTX2_MODEL_ID=Lightricks/LTX-Video-2B-T2V-30Steps
LTX2_OFFLOAD_DIR=/app/ltx2_offload
LTX2_USE_FP8_WEIGHT_ONLY=true
LTX2_ENABLE_TILING=true
LTX2_NUM_INFERENCE_STEPS=30
LTX2_GUIDANCE_SCALE=5.0
EOF
echo "✅ Created .env"

# Create docker-compose.ltx2.yml (GPU-enabled)
cat > docker-compose.ltx2.yml <<'EOF'
name: aivideoworker-ltx2
services:
  app:
    build:
      context: .
      dockerfile: Dockerfile.ltx2
    image: aivideoworker:ltx2
    container_name: aivideoworker-ltx2
    runtime: nvidia
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: all
              capabilities: [gpu]
    environment:
      - NVIDIA_VISIBLE_DEVICES=all
      - NVIDIA_DRIVER_CAPABILITIES=compute,utility
      - VIDEO_OUTPUT_DIR=/app/output
      - IMAGE_DIR=/app/input
      - LTX2_MODEL_ID=${LTX2_MODEL_ID}
      - LTX2_NUM_INFERENCE_STEPS=${LTX2_NUM_INFERENCE_STEPS}
      - LTX2_ENABLE_TILING=${LTX2_ENABLE_TILING}
      - LTX2_USE_FP8_WEIGHT_ONLY=${LTX2_USE_FP8_WEIGHT_ONLY}
      - LTX2_OFFLOAD_DIR=/app/ltx2_offload
    ports:
      - "8000:8000"
    volumes:
      - ./output:/app/output
      - ./input:/app/input
      - ltx2-model-cache:/root/.cache/huggingface
    restart: unless-stopped

volumes:
  ltx2-model-cache:
EOF
echo "✅ Created docker-compose.ltx2.yml"

# Create Dockerfile.ltx2
cat > Dockerfile.ltx2 <<'EOF'
FROM pytorch/pytorch:2.5.1-cuda12.4-cudnn9-runtime

WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends \
    git curl libglib2.0-0 libsm6 libxext6 libxrender1 ffmpeg \
    && rm -rf /var/lib/apt/lists/*

COPY requirements.txt requirements-ltx2.txt ./
RUN pip install --no-cache-dir -r requirements.txt -r requirements-ltx2.txt

COPY app/ ./app/
COPY run.py ./

EXPOSE 8000
CMD ["python", "run.py"]
EOF
echo "✅ Created Dockerfile.ltx2"

# Create requirements-ltx2.txt
cat > requirements-ltx2.txt <<'EOF'
diffusers>=0.29
transformers>=4.44
accelerate
xformers
torch>=2.5.1
torchvision
opencv-python
pillow
av
fastapi[all]
uvicorn
python-multipart
requests
tqdm
numpy
EOF
echo "✅ Created requirements-ltx2.txt"

# Create directories
mkdir -p output input
echo "✅ Created output and input directories"

echo ""
echo "🎉 Setup complete!"
echo ""
echo "To start the service:"
echo "  docker compose -f docker-compose.ltx2.yml up --build"
echo ""
echo "First run will download LTX model (~3-5GB). Subsequent runs use cache."
