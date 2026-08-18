"""Property-viewing slideshow generator (Ken Burns + cross-dissolve).

Used as the fallback path when more than one image is uploaded, or when the
Stable Video Diffusion model is not available.  It turns a list of property
photos into a smooth, shareable video tour:

  * every photo is zoomed/panned (Ken Burns effect),
  * consecutive photos cross-dissolve into each other,
  * all photos are normalised to a single canvas so the video is steady.

The actual encoding is delegated to
:func:`app.services.video_generator.write_frames_video`.
"""
from __future__ import annotations

import io
import logging
import random
from pathlib import Path
from typing import List, Optional, Sequence, Tuple

import cv2
import numpy as np
from PIL import Image

from app.core.config import settings
from app.services.video_generator import write_frames_video

logger = logging.getLogger("aivideoworker.slideshow")


def _bgr_from_any(data: bytes) -> np.ndarray:
    """Decode raw image bytes into a BGR uint8 ndarray (OpenCV convention)."""
    arr = np.frombuffer(data, dtype=np.uint8)
    img = cv2.imdecode(arr, cv2.IMREAD_COLOR)
    if img is None:
        # Fall back to Pillow for formats OpenCV struggles with (e.g. some webp).
        pil = Image.open(io.BytesIO(data)).convert("RGB")
        img = np.array(pil)[:, :, ::-1].copy()
    return img


def _cover_resize(src: np.ndarray, canvas_w: int, canvas_h: int) -> np.ndarray:
    """Resize ``src`` so it fully covers ``canvas_w`` x ``canvas_h`` while
    keeping the aspect ratio.  Both output dimensions are >= canvas, which
    gives the Ken Burns effect room to pan/zoom inside the photo.
    """
    h, w = src.shape[:2]
    scale = max(canvas_w / w, canvas_h / h, 1.0)
    nw = max(canvas_w, int(round(w * scale)))
    nh = max(canvas_h, int(round(h * scale)))
    return cv2.resize(src, (nw, nh), interpolation=cv2.INTER_AREA)


def _interpolate_roi(
    a: Tuple[int, int, int, int], b: Tuple[int, int, int, int], t: float
) -> Tuple[int, int, int, int]:
    """Linearly interpolate two (x, y, w, h) regions of interest."""
    xa, ya, wa, ha = a
    xb, yb, wb, hb = b
    return (
        int(round(xa + (xb - xa) * t)),
        int(round(ya + (yb - ya) * t)),
        max(1, int(round(wa + (wb - wa) * t))),
        max(1, int(round(ha + (hb - ha) * t))),
    )


def _ken_burns_roi(
    sw: int, sh: int, canvas_w: int, canvas_h: int, scale: float, rng: random.Random
) -> Tuple[int, int, int, int]:
    """Pick a random source ROI for a given zoom ``scale`` (1.0 = full frame)."""
    rw = max(1, int(round(canvas_w / scale)))
    rh = max(1, int(round(canvas_h / scale)))
    rw = min(rw, sw)
    rh = min(rh, sh)
    x = rng.randint(0, max(0, sw - rw))
    y = rng.randint(0, max(0, sh - rh))
    return (x, y, rw, rh)


def _ken_burns_frames(
    src: np.ndarray, canvas_w: int, canvas_h: int, num_frames: int, rng: random.Random
) -> List[np.ndarray]:
    """Render ``num_frames`` of Ken Burns (zoom + pan) from a cover-resized src."""
    sh, sw = src.shape[:2]

    zoom_min = max(0.5, min(0.99, settings.SLIDESHOW_ZOOM_MIN))
    zoom_max = max(zoom_min + 0.01, min(1.0, settings.SLIDESHOW_ZOOM_MAX))

    scale_start = 1.0
    scale_end = rng.uniform(zoom_min, zoom_max)  # zoom in over the segment

    roi_start = _ken_burns_roi(sw, sh, canvas_w, canvas_h, scale_start, rng)
    roi_end = _ken_burns_roi(sw, sh, canvas_w, canvas_h, scale_end, rng)

    frames: List[np.ndarray] = []
    for i in range(num_frames):
        t = i / max(1, num_frames - 1) if num_frames > 1 else 0.0
        x, y, rw, rh = _interpolate_roi(roi_start, roi_end, t)
        x = min(max(0, x), max(0, sw - rw))
        y = min(max(0, y), max(0, sh - rh))
        crop = src[y:y + rh, x:x + rw]
        frames.append(
            cv2.resize(crop, (canvas_w, canvas_h), interpolation=cv2.INTER_AREA)
        )
    return frames


def _adjust_length(frames: List[np.ndarray], total: int) -> List[np.ndarray]:
    """Trim or hold the last frame so the segment is exactly ``total`` frames."""
    if not frames:
        return frames
    if len(frames) >= total:
        return frames[:total]
    last = frames[-1]
    frames.extend([last] * (total - len(frames)))
    return frames[:total]


def generate_slideshow(
    images: Sequence[Tuple[str, bytes]],
    output_path: Path,
    duration: float,
    fps: int = 24,
    canvas_w: Optional[int] = None,
    canvas_h: Optional[int] = None,
    transition_duration: Optional[float] = None,
) -> Path:
    """Build a Ken Burns + cross-dissolve property tour from ``images``.

    Args:
        images: sequence of ``(filename, raw_bytes)`` tuples (already validated).
        output_path: destination ``.mp4`` / ``.webm`` path.
        duration: total video length in seconds.
        fps: frames per second.
        canvas_w/canvas_h: output resolution (defaults to settings).
        transition_duration: seconds of cross-dissolve between photos.
    """
    if not images:
        raise ValueError("At least one image is required")

    canvas_w = int(canvas_w or settings.SLIDESHOW_WIDTH)
    canvas_h = int(canvas_h or settings.SLIDESHOW_HEIGHT)
    if transition_duration is None:
        transition_duration = settings.SLIDESHOW_TRANSITION_DURATION

    covers: List[np.ndarray] = [
        _cover_resize(_bgr_from_any(d), canvas_w, canvas_h) for _, d in images
    ]

    rng = random.Random(settings.SLIDESHOW_SEED)
    total_frames = max(1, int(round(duration * fps)))
    n = len(covers)

    # Single photo -> just Ken Burns across the whole timeline.
    if n == 1:
        frames = _ken_burns_frames(covers[0], canvas_w, canvas_h, total_frames, rng)
        return write_frames_video(frames, output_path, fps, canvas_w, canvas_h)

    trans_frames = max(
        1, min(int(round(transition_duration * fps)), max(1, total_frames // 3))
    )
    per_image = max(trans_frames + 2, total_frames // n)
    segs = [_ken_burns_frames(c, canvas_w, canvas_h, per_image, rng) for c in covers]

    out: List[np.ndarray] = []
    for i in range(n):
        seg = segs[i]
        cut = per_image - trans_frames
        out.extend(seg[:cut])
        if i < n - 1:
            nxt = segs[i + 1]
            for k in range(trans_frames):
                alpha = (k + 1) / trans_frames
                out.append(
                    cv2.addWeighted(seg[cut + k], 1.0 - alpha, nxt[k], alpha, 0)
                )

    out = _adjust_length(out, total_frames)
    logger.info(
        "Slideshow done: %d frames, %dx%d, %d images -> %s",
        len(out), canvas_w, canvas_h, n, output_path,
    )
    return write_frames_video(out, output_path, fps, canvas_w, canvas_h)