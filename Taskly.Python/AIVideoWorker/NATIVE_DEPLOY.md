# AIVideoWorker — Native Deployment (no Docker)

This guide runs the whole AIVideoWorker FastAPI service **directly on the VPS**
inside a Python venv, with no containers and no nvidia-container-toolkit. It is
the bare-metal equivalent of the `Dockerfile` / `docker-compose.yml` / deployed
`deploy.sh` stack you were using.

If you were hitting container/GPU problems (CUDA not exposed inside the
container, driver matching, compose/GPU reservation), this removes that layer
entirely: the venv uses the host's real CUDA driver directly.

---

## 1. Why the containers were failing (quick recap)

| Docker problem | What the native setup does instead |
| --- | --- |
| `runtime: nvidia` / driver cannot be exposed to the container | Not used — uvicorn reads the host CUDA driver directly |
| torch build / driver CUDA mismatch → `torch.cuda.is_available() == False` | Pin `torch==2.5.1+cu124` + `torchvision==0.20.1+cu124` (same as `Dockerfile.ltx2`) |
| headless OpenCV missing `libgl1` / `libglib2.0-0` | `apt-get` installs them on the host |
| container rebuild each time deps change | venv is created once, additive afterwards |

The service itself runs **exactly the same**: `uvicorn app.main:app` on port
`8000`. Nothing about the API, endpoints, or storage paths changes.

---

## What you'll get

- `app/` FastAPI service → `http://0.0.0.0:8000`
  - `GET  /health` → liveness + deployment status
  - `POST /api/video/generate` (multipart images)
  - `POST /api/video/generate-json` (JSON + base64 images)
  - `POST /api/video/generate-text-video` (text → video)
  - `GET  /api/video/status/{job_id}`, `GET  /api/video/download/{job_id}`
- AI image-to-video via the **diffusers LTX-Video** backend (default).
- Ken Burns + cross-dissolve slideshow fallback for multiple photos / no GPU.
- The real **LTX-2** engine (`ltx_pipelines` / `ltx-core`) is *optional* and stays
  lazy; it is OFF in the native `.env` so the app boots cleanly without it.

> ⚠️ Fetch this project to the VPS first. If you haven't yet:
> ```bash
> git clone https://github.com/XhantiMzozoyana123/Taskly
> cd Taskly/Taskly.Python/AIVideoWorker
> ```

---

## 2. Install everything (venv + CUDA torch + app deps + `.env`)

```bash
cd Taskly/Taskly.Python/AIVideoWorker
bash deploy_native.sh install
```

This does, in order:

1. Installs system libs headless OpenCV needs: `libgl1`, `libglib2.0-0`, plus
   `ffmpeg`, `git`, `curl`.
2. Creates `.venv` and activates it.
3. Installs the **CUDA-12.4** torch/torchvision wheels from PyTorch's index:
   ```bash
   python -m pip install --extra-index-url https://download.pytorch.org/whl/cu124 \
       torch==2.5.1+cu124 torchvision==0.20.1+cu124
   ```
4. Installs the rest of `requirements.txt`.
5. Creates `generated/`, `output/`, `input/`.
6. Writes a **native-profile `.env`** (only if one doesn't exist yet).
7. Prints whether `torch.cuda.is_available()` is `True`.

> First run downloads the LTX video model (~4-8 GB for `Lightricks/LTX-Video`)
> into the Hugging Face cache the first time a job uses it — exactly like the
> Docker path did.

## 3. Configure `.env` (before starting)

Open `.env` in the `AIVideoWorker/` folder and check these:

```dotenv
HOST=0.0.0.0
PORT=8000
AI_DEVICE=cuda            # or "cpu" if you have no GPU
AI_MODEL=ltx              # or "svd"
TOUR_STYLE=ai
LTX2_ENABLED=false        # leave false unless you install the LTX-2 stack
HF_TOKEN=                 # set only for gated models / LTX-2 weights
```

The default profile works as-is on a CUDA GPU box. If you want Stable Video
Diffusion instead of LTX, set `AI_MODEL=svd`.

## 4. Start the server

**Option A — foreground (quick test):**

```bash
bash deploy_native.sh run
# or, if already installed:
.venv/bin/python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
```

**Option B — background with systemd (recommended for a VPS).** Create
`/etc/systemd/system/aivideoworker.service`:

```ini
[Unit]
Description=AIVideoWorker (FastAPI video service)
After=network.target

[Service]
Type=python
WorkingDirectory=/path/to/Taskly/Taskly.Python/AIVideoWorker
ExecStart=/path/to/Taskly/Taskly.Python/AIVideoWorker/.venv/bin/python -m uvicorn app.main:app --host 0.0.0.0 --port 8000
Restart=on-failure

[Install]
WantedBy=multi-user
```

Then:

```bash
sudo systemctl enable aivideoworker
sudo systemctl start aivideoworker
```

(Replace `/path/to/...` with your actual path.)

## 5. Smoke test

```bash
curl http://127.0.0.1:8000/health
# {"status":"healthy","service":"AIVideoWorker","deployment":{...}}
```

The `deployment` object reports `torch_cuda_available`, the detected GPU name,
and the model/tour settings. Keep port 8000 open in the VPS firewall.

> Shortcut for the whole thing on a fresh box:
> ```bash
> bash deploy_native.sh install && bash deploy_native.sh run
> ```
## 6. Troubleshooting (Docker problems → native fixes)

| Symptom | Cause | Fix |
| --- | --- | --- |
| `torch.cuda.is_available()` is `False` in `/health` | Unpinned torch fetched a CUDA-13 build your driver can't expose | Re-run the script (it pins `2.5.1+cu124`); confirm driver supports CUDA 12.4 |
| `No module named 'av'` / PyAV import error | `PyAV` needs its bundled FFmpeg libs present | `sudo apt-get install -y ffmpeg` then re-run `pip install -r requirements.txt` |
| `cv2.imdecode` returns `None` / OpenCV can't decode | headless OpenCV missing `libgl1`/`libglib2.0-0` | `sudo apt-get install -y libgl1 libglib2.0-0` |
| model download fails | Gated repo needs auth | set `HF_TOKEN` in `.env` and accept the model license on HF |
| ports not reachable | container had firewall-less networking; bare install doesn't | open port 8000, and set `HOST=0.0.0.0` |
| slow / CPU only | no GPU present | use `AI_DEVICE=cpu` + `TOUR_STYLE=slideshow` and expect slow inference |

## 7. Optional: enable the real LTX-2 engine

The default native profile uses the **diffusers `Lightricks/LTX-Video`**
backend, which needs no extra packages and is what makes `/generate` +
`/generate-text-video` work out of the box. The real **LTX-2** engine in
`app/services/ltx2_engine.py` requires the private `ltx_pipelines` / `ltx-core`
Python packages (Lightricks), Python 3.12+, a ≥16 GB VRAM GPU, and the gated
LTX-2 (2.5/2.3) checkpoints on disk.

When you have those, set `LTX2_ENABLED=true` + the `LTX2_*` paths in `.env`;
the engine auto-selects itself when `ltx2_available()` is true and otherwise the
code falls back to the diffusers LTX backend — nothing else needs to change.