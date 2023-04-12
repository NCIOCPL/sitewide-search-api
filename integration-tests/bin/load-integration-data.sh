#!/bin/bash

## Use the Environment var or the default
if [[ -z "${ELASTIC_SEARCH_HOST}" ]]; then
    ELASTIC_HOST="http://localhost:9200"
else
    ELASTIC_HOST=${ELASTIC_SEARCH_HOST}
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

## Wait until docker is up.
echo "Waiting for ES Service at ${ELASTIC_HOST} to Start"
until $(curl --output /dev/null --silent --head --fail "${ELASTIC_HOST}"); do
    printf '.'
    sleep 1
done
echo "ES Service is up"

# First wait for ES to start...
response=$(curl --write-out %{http_code} --silent --output /dev/null "${ELASTIC_HOST}")

until [ "$response" = "200" ]; do
    response=$(curl --write-out %{http_code} --silent --output /dev/null "${ELASTIC_HOST}")
    >&2 echo "Elastic Search is unavailable - sleeping"
    sleep 1
done

# next wait for ES status to turn to Green
health_check="curl -fsSL ${ELASTIC_HOST}/_cat/health?h=status"
health=$(eval $health_check)
echo "Waiting for ES status to be ready"
until [[ "$health" = 'green' ]]; do
    >&2 echo "Elastic Search is unavailable - sleeping"
    sleep 10
    health=$(eval $health_check)
done
echo "ES status is green"

pushd $DIR/../data/source
echo "Load the index mappings"
echo "...for search"
MAPPING_LOAD_CMD="curl -fsS -H \"Content-Type: application/x-ndjson\" -XPUT ${ELASTIC_HOST}/cgov --data-binary \"@cgov-mapping.json\""
mapping_output=$(eval $MAPPING_LOAD_CMD)
echo $mapping_output

echo "...for autosuggest"
MAPPING_LOAD_CMD="curl -fsS -H \"Content-Type: application/x-ndjson\" -XPUT ${ELASTIC_HOST}/autosg --data-binary \"@autosg-mapping.json\""
mapping_output=$(eval $MAPPING_LOAD_CMD)
echo $mapping_output


# Combining the cgov-data files causes the bulk load to fail.
echo "Load the records"

for filename in cgov-data-{1..3}.jsonl regression-data.jsonl autosg-data.jsonl; do
    echo -n "Loading $filename... "
    BULK_LOAD_CMD="curl -fsS -H \"Content-Type: application/x-ndjson\" -XPOST ${ELASTIC_HOST}/_bulk --data-binary \"@$filename\""
    load_output=$(eval $BULK_LOAD_CMD)

    ## Test to make sure we loaded items
    HAS_ERRORS=$(echo $load_output | jq '.errors')
    if [[ "$HAS_ERRORS" = 'false' ]];
    then
        echo 'success.'
    else
        echo 'FAIL'
        echo $load_output | jq '.items[].index.error | select(. != null)'
        break
    fi
done


popd
