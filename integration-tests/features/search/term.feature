Feature: Search Term

    Background:
        * url apiHost

    Scenario: No search term provided should result in no results

        Given path 'Search', 'cgov', 'en', ''
        When method get

        # Currently API returns 200 with an empty JSON result
        # Swagger says term is required, which would imply a 400. We can update this if we ever switch it.
        Then status 200
        And match response == read('term.expected/search-no-hits.json')

    Scenario: Nonsense term should result in no results

        Given path 'Search', 'cgov', 'en', 'aoeuasdf'
        When method get
        Then status 200
        And match response == read('term.expected/search-no-hits.json')

    Scenario Outline: Images do not appear in the results when searching for general terms.
        for collection: <collection>, lang: <lang>, term: <term>, from: from, size: <size>, site: <site>

        Given path 'Search', collection, lang, term
        And param from = from
        And param size = size
        And param site = site
        When method get
        Then status 200
        And match each $.results[*].url == '#regex ^(?!.*[.](jpeg|jpg|gif|tiff|psd|svg|raw)$).*$'

        Examples:
            | collection | lang | term              | from | size | site                                           |
            | cgov       | en   | breast            | 0    | 1000 | all                                            |
            | cgov       | es   | mama              | 0    | 1000 | all                                            |
            | doc        | en   | physical activity | 0    | 1000 | dceg.cancer.gov                                |
            | doc        | en   | nanotechnology    | 0    | 1000 | www.cancer.gov/nano                            |
            | doc        | en   | meningioma        | 0    | 1000 | www.cancer.gov/rare-brain-spine-tumor          |
            | doc        | es   | glioblastoma      | 0    | 1000 | www.cancer.gov/rare-brain-spine-tumor/espanol  |

    Scenario Outline: Images do not appear in the results when searching for image-specific terms.
        for collection: <collection>, lang: <lang>, term: <term>, from: from, size: <size>, site: <site>

        Given path 'Search', collection, lang, term
        And param from = from
        And param size = size
        And param site = site
        When method get
        Then status 200
        And match each $.results[*].url == '#regex ^(?!.*[.](jpeg|jpg|gif|tiff|psd|svg|raw)$).*$'

        Examples:
            | collection | lang | term | from | size | site                                          |
            | cgov       | en   | gif  | 0    | 1000 | all                                           |
            | cgov       | es   | svg  | 0    | 1000 | all                                           |
            | doc        | en   | png  | 0    | 1000 | dceg.cancer.gov                               |
            | doc        | en   | jpg  | 0    | 1000 | www.cancer.gov/nano                           |
            | doc        | en   | jpeg | 0    | 1000 | www.cancer.gov/rare-brain-spine-tumor         |
            | doc        | es   | psd  | 0    | 1000 | www.cancer.gov/rare-brain-spine-tumor/espanol |
