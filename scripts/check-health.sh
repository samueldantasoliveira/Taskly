#!/usr/bin/env bash
set -euo pipefail
url="${1:?Usage: bash scripts/check-health.sh https://api.example/health/ready}"
[[ "$url" == https://* || "$url" == http://localhost:* || "$url" == http://127.0.0.1:* ]] || {
  echo 'Use HTTPS or local HTTP.' >&2; exit 1;
}
for attempt in {1..3}; do
  status="$(curl --silent --output /dev/null --write-out '%{http_code}' --connect-timeout 10 --max-time 60 "$url" || true)"
  if [[ "$status" == 200 ]]; then echo 'API ready (HTTP 200).'; exit 0; fi
  echo "Health attempt $attempt failed (HTTP $status)." >&2
  if [[ "$attempt" != 3 ]]; then sleep 10; fi
done
exit 1
