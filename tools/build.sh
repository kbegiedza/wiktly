#!/bin/bash

set -e

docker build -t wiktly/api:local -f services/backend/api.Dockerfile services/backend
