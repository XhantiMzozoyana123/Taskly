"""Standalone tests for the AI-model dispatch (LTX / SVD) in video_generator.

Runs WITHOUT a GPU: the real diffusion pipelines are faked, so we validate the
dispatch, the LTX->SVD fallback, and the frame->MP4 encoding logic locally.

Run:  python tests/test_ltx_dispatch.py
"""
import io
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]  # AIVideoWorker/
sys.path.insert(0, str(ROOT))

from app.core.config import settings  # noqa: E402
from app.services import video_generator  # noqa: E402
from app.services.slideshow import generate_slideshow  # noqa: E402


class _FakeResult:
    def __init__(self, frames):
        self.frames = [frames]


class FakePipe:
    """Stand-in for an LTX / SVD pipeline: pipe(image=..., prompt=..., **kw)."""

    def __init__(self, fail: bool = False, n_frames: int = 8):
        self.fail = fail
        self.n_frames = n_frames
        self.calls = 0

    def __call__(self, **kwargs):
        self.calls += 1
        if self.fail:
            raise RuntimeError("simulated pipeline failure")
        image = kwargs["image"]
        w, h = image.size
        frames = [
            Image.new("RGB", (w, h), (200 - i * 10, 100, 50 + i * 10))
            for i in range(self.n_frames)
        ]
        return _FakeResult(frames)


def _jpeg_bytes(width: int = 640, height: int = 480, color=(12, 34, 56)) -> bytes:
    buf = io.BytesIO()
    Image.new("RGB", (width, height), color).save(buf, format="JPEG")
    return buf.getvalue()


def _count_frames(path: Path) -> int:
    import av

    container = av.open(str(path))
    stream = container.streams.video[0]
    count = sum(1 for _ in container.decode(stream))
    container.close()
    return count


def _seed_pipes(model_setting, ltx_fails):
    video_generator._pipeline_cache.clear()
    settings.AI_MODEL = model_setting
    # Avoid importing torch just to build a torch.Generator.
    settings.generator = lambda: None
    video_generator._pipeline_cache["ltx"] = FakePipe(fail=True) if ltx_fails else FakePipe(fail=False)
    video_generator._pipeline_cache["svd"] = FakePipe(fail=False)
    return video_generator._pipeline_cache["ltx"], video_generator._pipeline_cache["svd"]


def _run(model_setting, ltx_fails):
    ltx_pipe, svd_pipe = _seed_pipes(model_setting, ltx_fails)
    out = Path("generated/_ltx_unit_test.mp4")
    out.parent.mkdir(parents=True, exist_ok=True)
    out.unlink(missing_ok=True)
    video_generator.generate_image_to_video(
        _jpeg_bytes(640, 480), out, prompt="property tour", model=model_setting
    )
    assert out.exists(), "output video was not written"
    assert out.stat().st_size > 0, "output video is empty"
    frames = _count_frames(out)
    assert frames > 0, "output video has no frames"
    out.unlink(missing_ok=True)
    return ltx_pipe, svd_pipe, frames


def test_ltx_selected():
    ltx, svd, frames = _run("ltx", ltx_fails=False)
    assert ltx.calls == 1, "LTX pipeline should be used"
    assert svd.calls == 0, "SVD should not be called when LTX succeeds"
    print(f"  [ok] LTX success -> frames={frames}, ltx_calls={ltx.calls}, svd_calls={svd.calls}")


def test_ltx_fallback_to_svd():
    ltx, svd, frames = _run("ltx", ltx_fails=True)
    assert ltx.calls == 1, "LTX should have been attempted"
    assert svd.calls == 1, "SVD should be used as the fallback"
    print(f"  [ok] LTX failure -> SVD fallback -> frames={frames}, ltx_calls={ltx.calls}, svd_calls={svd.calls}")


def test_explicit_svd():
    ltx, svd, frames = _run("svd", ltx_fails=False)
    assert ltx.calls == 0
    assert svd.calls == 1
    print(f"  [ok] explicit SVD -> frames={frames}, ltx_calls={ltx.calls}, svd_calls={svd.calls}")


def test_multi_image_slideshow():
    """Regression: the multi-image property-tour (Ken Burns) path still encodes."""
    out = Path("generated/_slideshow_smoke.mp4")
    out.parent.mkdir(parents=True, exist_ok=True)
    out.unlink(missing_ok=True)
    images = [
        ("room1.jpg", _jpeg_bytes(640, 480, (220, 180, 140))),
        ("room2.jpg", _jpeg_bytes(640, 480, (180, 200, 220))),
        ("room3.jpg", _jpeg_bytes(640, 480, (160, 120, 90))),
    ]
    path = generate_slideshow(
        images=images, output_path=out, duration=3.0, fps=24, transition_duration=1.0
    )
    assert path.exists(), "slideshow output not written"
    assert path.stat().st_size > 0
    frames = _count_frames(out)
    assert frames > 0
    out.unlink(missing_ok=True)
    print(f"  [ok] multi-image slideshow -> frames={frames}")


if __name__ == "__main__":
    import logging

    logging.basicConfig(level=logging.INFO)
    test_ltx_selected()
    test_ltx_fallback_to_svd()
    test_explicit_svd()
    test_multi_image_slideshow()
    print("ALL TESTS PASSED")
