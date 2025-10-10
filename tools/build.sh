#!/bin/bash

set -e

docker build -t wiktly/api:local -f services/backend/api.Dockerfile services/backend
docker build -t wiktly/web:local -f services/backend/web.Dockerfile services/backend
