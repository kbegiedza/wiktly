#!/bin/bash

set -e

docker run -p 7001:8080 -p 7002:8081 --name wiktly-api -d --rm wiktly/api:local
docker run -p 7080:8080 -p 7443:8081 --name wiktly-web -d --rm wiktly/web:local
