#!/usr/bin/env bash
# set -Eeo pipefail

# Example using the functions of the MariaDB entrypoint to customize startup to always run files in /always-initdb.d/

source "$(which docker-entrypoint.sh)"

# docker_setup_env "$@"

docker_process_init_files /docker-entrypoint-initdb.d/*/*/*