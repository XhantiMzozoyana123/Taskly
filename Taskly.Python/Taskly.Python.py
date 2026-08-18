"""Launch the AIVideoWorker Python app.

This script simply runs the AIVideoWorker's own launcher (run.py) with
the current Python interpreter, using the AIVideoWorker folder as the
working directory so its relative paths resolve correctly.

Usage:
    python Taskly.Python.py
"""
import os
import subprocess
import sys

# The AIVideoWorker folder lives next to this file.
HERE = os.path.dirname(os.path.abspath(__file__))
AIVIDEO_WORKER_DIR = os.path.join(HERE, "AIVideoWorker")
RUN_SCRIPT = os.path.join(AIVIDEO_WORKER_DIR, "run.py")

if __name__ == "__main__":
    sys.exit(
        subprocess.call(
            [sys.executable, RUN_SCRIPT],
            cwd=AIVIDEO_WORKER_DIR,
        )
    )
