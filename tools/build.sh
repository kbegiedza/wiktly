#!/bin/bash

set -e

docker build -t wiktly:local -f services/api/Dockerfile services/api
