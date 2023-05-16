#!/bin/bash

# This script is used by the GitHub CI workflow in order to perform
# API-specific setup of the Docker container.  (E.g. copying files.)

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
echo $DIR

if [[ -z $1 ]]; then
    echo 'Container ID not specified.'
    exit 1
fi

CONTAINER_ID=$1
docker cp "../docker-sws-api/elasticsearch/synonym.txt"  "$CONTAINER_ID:/usr/share/elasticsearch/config/synonym.txt"
