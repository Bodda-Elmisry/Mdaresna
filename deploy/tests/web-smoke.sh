#!/bin/sh
set -eu
test "$(id -u)" -ne 0
/docker-entrypoint.sh nginx -g 'daemon off;' &
nginx_process=$!
trap 'kill "$nginx_process" 2>/dev/null || true' EXIT
attempt=0
until wget -q -O /tmp/health http://127.0.0.1:8080/health; do
    attempt=$((attempt + 1))
    test "$attempt" -lt 10
    sleep 1
done
grep -q healthy /tmp/health
wget -q -O /tmp/index.html http://127.0.0.1:8080/
grep -q '<html' /tmp/index.html
wget -q -O /tmp/deep.html http://127.0.0.1:8080/a/deep/client/route
cmp /tmp/index.html /tmp/deep.html
wget -q -O /tmp/env.js http://127.0.0.1:8080/env.js
grep -q 'https://fixture.example.invalid' /tmp/env.js
echo 'Non-root/read-only web HTTP, SPA fallback and runtime env.js smoke passed.'
