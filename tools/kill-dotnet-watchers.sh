#!/bin/bash

set -e

kill -9 $(ps aux | grep 'dotnet watch' | grep -v grep | awk '{print $2}')
