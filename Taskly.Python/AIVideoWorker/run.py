"""Run the AIVideoWorker application with Uvicorn.

Usage:
    python run.py
"""
import os
import sys

# Ensure the project root is on sys.path so 'app' is importable
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import uvicorn

if __name__ == "__main__":
    uvicorn.run(
        "app.main:app",
        host="127.0.0.1",
        port=8000,
        reload=True,
    )