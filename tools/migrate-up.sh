#!/bin/bash

set -e

dotnet ef database update \
  --project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --startup-project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --context Wiktly.Web.Persistence.EntityFramework.WiktlyDbContext \
  --configuration Debug \
  --verbose

dotnet ef database update \
  --project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --startup-project services/backend/src/Wiktly.Web/Wiktly.Web.csproj \
  --context Wiktly.Web.Areas.Identity.Data.AuthDataContext \
  --configuration Debug \
  --verbose