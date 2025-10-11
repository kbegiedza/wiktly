#!/bin/bash

NAME=$1

if [ -z "$NAME" ]; then
  echo "Usage: $0 <MigrationName>"
  exit 1
fi

set -e

dotnet ef migrations add \
  --project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --startup-project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --context Wiktly.Web.Areas.Identity.Data.AuthDataContext \
  --configuration Debug \
  --verbose \
  "${NAME}" \
  --output-dir Areas/Identity/Data/Migrations