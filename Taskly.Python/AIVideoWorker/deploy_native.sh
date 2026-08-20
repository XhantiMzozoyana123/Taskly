#!/usr/bin/env bash
#
# deploy_native.sh
# ---------------------------------------------------------------------------
# AIVideoWorker NATIVE deployment - no Docker containers.
#
# This replicates what the Dockerfiles/Docker runtime provided (CUDA torch
# wheels, CUDA-12.4 driver-matched torchvision, headless OpenCV libs, ffmpeg,
# HF model cache) but installs it DIRECTLY on the VPS in a venv and starts
# uvicorn bare-metal.  No nvidia-container-toolkit, no compose, no containers.
#
# Usage:
#   bash deploy_native.sh install   # install EVERYTHING (idempotent), then
#                                   # edit .env -> you start the server later
#   bash deploy_native.sh run       # install if needed, then START uvicorn (foreground)
#
# Requirements (on the VPS):
#   * Debian/Ubuntu (apt), NVIDIA GPU driver installed, ~16GB+ VRAM for the AI
#     image-to-video model. Python 3.10 / 3.11 / 3.12 (LTX-2 prefers 3.12+).
#   * Set HF_TOKEN in .env if you want to pull gated/gated LTX models.
#
# The docker path uses  torch==2.5.1+cu124 because the host driver exposes
# CUDA 12.4; an unpinned torch pulls a CUDA-13 build that the driver cannot
# expose -> torch.cuda.is_available() returns False. We do the same here.
# ---------------------------------------------------------------------------
set -e

APP_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$APP_DIR"

MODE="${1:-run}"
PYTHON="${PYTHON:-python3}"

echo "==> AIVideoWorker native deploy  (dir=$APP_DIR, mode=$MODE)"

# --------------------------------------------------------------------------
# 1) System libraries headless OpenCV still needs on a bare Linux box
#    (matches the Dockerfile's apt-get step).
# --------------------------------------------------------------------------
if command -v apt-get >/dev/null 2>&1; then
  echo "==> Installing system libs (libgl1, libglib2.0-0, ffmpeg, git, curl) ..."
  sudo apt-get update
  sudo apt-get install -y --no-install-recommends \
    git curl ffmpeg \
    libgl1 libglib2.0-0 \
    python3 python3-venv python3-pip python3-dev
else
  echo "!! apt-get not found - install the equivalent packages manually."
fi

# --------------------------------------------------------------------------
# 2) venv (isolate the exact dependency versions so we never fight the OS)
# --------------------------------------------------------------------------
if [ ! -d .venv ]; then
  echo "==> Creating .venv ..."
  "$PYTHON" -m venv .venv
fi
# shellcheck disable=SC1091
source .venv/bin/activate
python -m pip install --upgrade pip setuptools wheel

# --------------------------------------------------------------------------
# 3) PyTorch + torchvision CUDA-12.4 wheels (pinned like the Dockerfile.ltx2
#    path) before the rest, so pip treats the "torch" lines in requirements.txt
#    as already satisfied and will NOT fetch a CUDA-13 build.
# --------------------------------------------------------------------------
echo "==> Installing torch/torchvision cu124 wheel ..."
python -m pip install --extra-index-url https://download.pytorch.org/whl/cu124 \
  "torch==2.5.1+cu124" "torchvision==0.20.1+cu124"

# --------------------------------------------------------------------------
# 4) App dependencies (the rest of requirements.txt).
# --------------------------------------------------------------------------
echo "==> Installing app dependencies from requirements.txt ..."
python -m pip install -r requirements.txt

# --------------------------------------------------------------------------
# 5) Runtime directories (generated videos, LTX-2 input/output/offload).
# --------------------------------------------------------------------------
mkdir -p generated output input

# --------------------------------------------------------------------------
# 6) .env - create only if it does not already exist (do not clobber).
# --------------------------------------------------------------------------
if [ ! -f .env ]; then
  echo "==> Creating .env (native profile) ..."
  cat > .env <<'EOF'
# --- FastAPI server ---
HOST=0.0.0.0
PORT=8000
GENERATED_VIDEO_DIRECTORY=generated

# --- AI image-to-video (LTX-Video diffusers backend by default) ---
# cuda = NVIDIA GPU (fast) | cpu = no GPU (slow, Ken Burns fallback for tours)
AI_DEVICE=cuda
AI_MODEL_ID=stabilityai/stable-video-diffusion-img2vid
AI_USE_FP16=true
# RTX A4000 / 16 GB GPUs: 1280x720 LTX clips (~14.7 GiB working set) exceed the
# card's 15.63 GiB. Set this to true to stream the model to CPU between steps so
# 720p fits (it's ~2-3x slower). Leave false on 24+ GB cards for max speed.
AI_CPU_OFFLOAD=false
AI_NUM_FRAMES=14
AI_FPS=7
AI_DECODE_CHUNK_SIZE=8
AI_MOTION_BUCKET_ID=127
AI_NOISE_AUG_STRENGTH=0.02
AI_MAX_EDGE=1024
AI_SEED=42

# --- AI model selection ---
# ltx -> Lightricks LTX-Video (fast, OpenArt-style motion) | svd -> Stable Video Diffusion
AI_MODEL=ltx
AI_LTX_MODEL_ID=Lightricks/LTX-Video
AI_LTX_NUM_INFERENCE_STEPS=30
AI_LTX_FPS=24
AI_LTX_PROMPT=cinematic motion, smooth camera movement, high detail, 4k, ultra sharp
AI_LTX_NUM_FRAMES=0
# ai -> per-photo LTX clips crossfaded ; slideshow -> Ken Burns tour (no GPU)
TOUR_STYLE=ai

# --- Property-tour slideshow fallback ---
VIDEO_FORMAT=mp4
SLIDESHOW_WIDTH=1280
SLIDESHOW_HEIGHT=720
SLIDESHOW_TRANSITION_DURATION=1.0
SLIDESHOW_ZOOM_MIN=0.7
SLIDESHOW_ZOOM_MAX=1.15
SLIDESHOW_SEED=42

# --- LTX-2 engine (REAL Lightricks stack; optional) ---
# Off in the native profile: the diffusers LTX path above is used unless you
# additionally install the private ltx_pipelines/ltx-core packages.
LTX2_ENABLED=false
LTX2_CHECKPOINT_DIR=/opt/models/ltx2.5/transformer
LTX2_GEMMA_DIR=/opt/models/ltx2.5/gemma4-12b
LTX2_UPSCALER_PATH=/opt/models/ltx2.5/upscaler
LTX2_VIDEO_VAE_PATH=/opt/models/ltx2.5/video-vae
LTX2_AUDIO_VAE_PATH=/opt/models/ltx2.5/audio-vae
LTX2_DURATION_HEAD_PATH=/opt/models/ltx2.5/duration-head
LTX2_DURATION_SECONDS=4
LTX2_WIDTH=1280
LTX2_HEIGHT=720
LTX2_FPS=25
LTX2_SEED=42

# --- HuggingFace token (needed ONLY for gated models / LTX-2 weights) ---
HF_TOKEN=
EOF
fi

echo ""
echo "==> Installation complete."
python -c "import torch,sys; sys.exit(0) if torch.cuda.is_available() else sys.exit(1)" \
  && echo "    CUDA GPU detected: $(python -c 'import torch;print(torch.cuda.get_device_name(0))')" \
  || echo "    !! CUDA NOT detected: AI image-to-video will not run; the Ken Burns fallback will."

# --------------------------------------------------------------------------
# 7) Start the server (foreground). Run `bash deploy_native.sh install` instead
#    if you want to launch it yourself (see NATIVE_DEPLOY.md for a systemd unit).
# --------------------------------------------------------------------------
if [ "$MODE" = "run" ]; then
  echo "==> Starting AIVideoWorker uvicorn on 0.0.0.0:8000 ..."
  echo "    Press Ctrl+C to stop."
  python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
fi