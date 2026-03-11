Notes on the various data files.

# AutoSuggest.CGov.En.BreastCancer.json

Contains suggestions based on the keyword "breast"

```
POST /autosg/terms/_search/template
{
    "file" : "autosg_suggest_cgov_en",
    "params" : {
        "searchstring" : "breast",
        "my_size" : 20
    }
}
```

# Search.CGov.En.AbsentFields.json

Search results on CancerGov for the word breast, remvoing a different
element from the _source collection on each (only url is required to be present.)

Based on the query below, with manual editing.

```
POST cgov/doc/_search/template
{
    "file": "cgov_search_cgov_en",
    "params": {
        "my_value" : "breast cancer"
        ,"my_size" : 10
        , "my_from" : 0
        ,"my_fields": "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\""
        , "my_site": "all"
    }
}
```

# Search.CGov.En.BreastCancer.json

Search results on CancerGov for the word breast.

```
POST cgov/doc/_search/template
{

    "file": "cgov_search_cgov_en",
    "params": {
        "my_value" : "breast cancer"
        ,"my_size" : 10
        , "my_from" : 0
        ,"my_fields": "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\""
        , "my_site": "all"
    }
}
```

# Search.CGov.En.MetadataArray-Expected.json

Parsed version of Search.CGov.En.MetadataArray.json.
Aside from the data being present, the first entry, and
only the first entry, from the metata.description field
should be present.

# Search.CGov.En.MetadataArray.json

Excerpt of CancerGov results for the word cancer, with an array of results
for the metadata description (Elasticsearch 5 style).

# Search.CGov.En.MetadataNestedArray-Expected.json

Parsed version of Search.CGov.En.MetadataNestedArray.json.
Aside from the data being present, the first entry, and
only the first entry, from the metata.description field
should be present.

# Search.CGov.En.MetadataNestedArray.json

Excerpt of CancerGov results for the term "James Cherry" with an array of results
for the metadata description (Elasticsearch 7 style).

# Search.CGov.En.MetadataSingle-Expected.json

Parsed version of Search.CGov.En.MetadataSingle.json.
Only one value for metata.description field should be present.

# Search.CGov.En.MetadataSingle.json

Excerpt of CancerGov results for the word cancer, with single entries
for the metadata description.

```
POST cgov/doc/_search/template
{

    "file": "cgov_search_cgov_en",
    "params": {
        "my_value" : "cancer"
        ,"my_size" : 20000
        , "my_from" : 10000
        ,"my_fields": "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\""
        , "my_site": "all"
    }
}
```

# Search.CGov.En.NoResults.json

Empty search result set.

```
POST cgov/doc/_search/template
{
    "file": "cgov_search_cgov_en",
    "params": {
        "my_value" : "znsasazdazaz"
        ,"my_size" : 10
        , "my_from" : 0
        ,"my_fields": "\"url\", \"title\", \"metatag.description\", \"metatag.dcterms.type\""
        , "my_site": "all"
    }
}
```

