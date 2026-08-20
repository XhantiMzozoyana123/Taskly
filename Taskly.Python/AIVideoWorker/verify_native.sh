#!/usr/bin/env bash
# verify_native.sh - confirm AIVideoWorker is truly running + GPU + reachable.
# Run on the VPS after `systemctl start aivideoworker`.
set -u

HOST="${1:-127.0.0.1}"
PORT="${2:-8000}"
BASE="http://${HOST}:${PORT}"

RED()  { printf "\033[31m%s\033[0m\n" "$*"; }
GREEN() { printf "\033[32m%s\033[0m\n" "$*"; }
PASS=0
FAIL=0

check() {
  if eval "$2"; then GREEN "✓ $1"; PASS=$((PASS+1)); else RED "✗ $1"; FAIL=$((FAIL+1)); fi
}

echo "==> Verifying AIVideoWorker at ${BASE}"

check "port ${PORT} is listening" \
  "ss -tlnp 2>/dev/null | grep -q ':${PORT} ' || netstat -tlnp 2>/dev/null | grep -q ':${PORT} '"

check "/health responds" \
  "curl -fsS --max-time 5 ${BASE}/health >/tmp/.aw_h >/dev/null"

check "GPU + torch visible (/health 'deployment')" \
  "curl -fsS --max-time 5 ${BASE}/health | python3 -c \"import sys,json; d=json.load(sys.stdin); v=d.get('deployment',{}); assert v.get('torch_cuda_available') is True; print(v.get('torch_cuda_device','GPU'))\""

check "Swagger UI reachable (/docs)" \
  "curl -fsS --max-time 5 -o /dev/null ${BASE}/docs"

echo
echo "==> ${PASS} passed, ${FAIL} failed"
[ "${FAIL}" -eq 0 ] && echo "ALL CHECKS PASSED — service is up, GPU available, /docs live" || echo "SOME CHECKS FAILED — see journalctl -u aivideoworker -f"
exit "${FAIL}"
