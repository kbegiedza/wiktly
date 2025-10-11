#!/bin/bash
set -e

function create_user_and_database() {
local database=${1}
echo "Creating user and database '${database}'"
psql -v ON_ERROR_STOP=1 --username "${POSTGRES_USER}" --dbname "postgres" <<-EOSQL
CREATE USER ${database};
CREATE DATABASE ${database};
GRANT ALL PRIVILEGES ON DATABASE ${database} TO ${database};
EOSQL
}

if [ -n "${POSTGRES_ADDITIONAL_DATABASES}" ]; then
echo "Multiple database creation requested: ${POSTGRES_ADDITIONAL_DATABASES}"
for db in $(echo "${POSTGRES_ADDITIONAL_DATABASES}" | tr ',' ' '); do
create_user_and_database "${db}"
done
echo "Multiple databases created"
fi

exec "$@"