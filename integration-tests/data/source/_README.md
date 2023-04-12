# About the data files.

## Mapping file sources

* **autosg-mapping.json** - comes from [autosuggest_index.js](https://github.com/NCIOCPL/autosuggest_load/blob/master/autosuggest_index.js) in the [autosuggest_load](https://github.com/NCIOCPL/autosuggest_load/blob/master/autosuggest_index.js) repository.  Note that this is a JavaScript file and the mapping "file" is actually the contents of the script's `body` variable.
* **cgov-mapping.json** - comes from [cgovindexprod.json](https://github.com/NCIOCPL/elasticsearch_config/blob/es792/indexcreation/cgovindexprod.json) in the [elasticsearch_config](https://github.com/NCIOCPL/elasticsearch_config/) repository.


## Data Files:

* **cgov-data-*.jsonl** - Original "representative" set of search data.
* **regression-data-.jsonl** - Additional test cases added after bug fixes.
