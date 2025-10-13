# Wiktly Tools

This directory contains various scripts and tools used for building, deploying, and managing the Wiktly project.

Below is a brief overview of the key scripts available:

- `add-auth-migration.sh <MigrationName>`: Adds a new Entity Framework migration for the authentication data context (
  AuthDataContext) in the Wiktly.Web project.
- `add-migration.sh <MigrationName>`: Adds a new Entity Framework migration for the main data context (
  WiktlyDataContext) in the Wiktly.Web project.
- `build.sh`: Builds the Docker images for the API and web services locally (wiktly/api:local and wiktly/web:local).
- `kill-dotnet-watchers.sh`: Terminates any running `dotnet watch` processes.
- `local_up.sh`: Starts the local Docker Compose environment using the configuration in ops/local.yml.
- `migrate-up.sh`: Applies pending Entity Framework migrations to update the databases for both the main data context
  and the authentication data context.
- `run.sh`: Runs the Wiktly web application in a local Docker container, exposing ports 7080 (HTTP) and 7443 (HTTPS).
