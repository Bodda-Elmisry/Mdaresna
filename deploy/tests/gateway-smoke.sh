#!/bin/sh
set -eu
# Self-signed test-only certificate generated in this disposable container, never deployed.
apk add --no-cache openssl >/dev/null
mkdir -p /run/tls
openssl req -x509 -newkey rsa:2048 -nodes -days 1 -subj '/CN=fixture.example.invalid' \
    -keyout /run/tls/privkey.pem -out /run/tls/fullchain.pem >/dev/null 2>&1
export PLATFORM_DOMAIN=platform.example.invalid SCHOOLS_DOMAIN=schools.example.invalid
envsubst '${PLATFORM_DOMAIN} ${SCHOOLS_DOMAIN}' < /source/default.conf.template > /etc/nginx/conf.d/default.conf
nginx -t
grep -q 'client_max_body_size 54m' /etc/nginx/conf.d/default.conf
grep -q 'proxy_request_buffering off' /etc/nginx/proxy-common.conf
echo 'Gateway TLS configuration syntax and upload buffering checks passed (test certificate only).'
