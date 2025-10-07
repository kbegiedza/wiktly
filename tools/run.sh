#!/bin/bash

set -e

docker run -p 8080:8080 -p 8081:8081 --rm wiktly:local
