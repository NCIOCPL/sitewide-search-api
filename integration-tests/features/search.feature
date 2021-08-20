Feature: Search

    Background:
        * url apiHost

    Scenario: we need to confirm that the feature is available
        Given path 'Search', 'status'
        When method get
        Then status 200
        And match response == 'alive!'

    Scenario Outline: we want pages matching search terms
        for collection: <collection>, lang: <lang>, term: <term>, from: from, size: <size>, site: <site>

        Given path 'Search', collection, lang, term
        And param from = from
        And param size = size
        And param site = site
        When method get
        Then status 200
        And match response == read('search-' + expected + '.json')

        Examples:
            | collection | lang | term         | from | size | site     | expected                     |
            | cgov       | en   | liver        | 0    | 5    | all      | cgov-en-liver-0-5-all        |
            | cgov       | en   | LIVER        | 0    | 5    | all      | cgov-en-liver-0-5-all        |
            | cgov       | en   | sezary       | 0    | 5    | all      | cgov-en-sezary-0-5-all       |
            | cgov       | en   | sézary       | 0    | 5    | all      | cgov-en-sezary-0-5-all       |
            | cgov       | en   | Sezary       | 0    | 5    | all      | cgov-en-sezary-0-5-all       |
            | cgov       | en   | Sézary       | 0    | 5    | all      | cgov-en-sezary-0-5-all       |
            | cgov       | en   | liver        | 1    | 5    | all      | cgov-en-liver-1-5-all        |
            | cgov       | en   | liver        | 1    | 3    | all      | cgov-en-liver-1-3-all        |
            | cgov       | en   | liver onions | 0    | 5    | all      | cgov-en-liver+onions-0-5-all |
            | cgov       | en   | liver        |      | 5    | all      | cgov-en-liver-0-5-all        |
            | cgov       | en   | liver        | 0    |      | all      | cgov-en-liver-0-10-all       |
            | cgov       | en   | liver        |      |      | all      | cgov-en-liver-0-10-all       |
            | cgov       | en   | bazooka      | 0    | 5    | all      | no-hits                      |
            | cgov       | en   |              | 0    | 5    | all      | no-hits                      |
            | cgov       | en   | liver        | 0    | 5    |          | cgov-en-liver-0-5-all        |
            | cgov       | en   | liver        |      |      |          | cgov-en-liver-0-10-all       |
            | cgov       | en   | liver        | 0    | 5    | dceg.    | cgov-en-liver-0-5-all        |
            | cgov       | en   | liver        | 0    | 5    | dada.    | cgov-en-liver-0-5-all        |
            | cgov       | es   | liver        | 0    | 5    | all      | cgov-es-liver-0-5-all        |
            | cgov       | es   | corazón      | 0    | 5    | all      | cgov-es-corazón-0-5-all      |
            | cgov       | es   | corazon      | 0    | 5    | all      | cgov-es-corazón-0-5-all      |
            | doc        | en   | liver        | 0    | 5    | all      | no-hits                      |
            | doc        | en   | liver        | 0    | 5    | dceg.    | doc-en-liver-0-5-dceg        |
            | doc        | en   | liver        | 0    | 5    | dada.    | no-hits                      |
            | doc        | es   | corazón      | 0    | 5    | dceg.    | no-hits                      |
            | doc        | en   | liver        | 0    | 5    | deainfo. | doc-en-liver-0-5-deainfo     |

    Scenario: site is used to restrict pages to one specific microsite
        Given path 'Search', 'doc', 'en', 'cancer'
        And param size = 1000
        And param site = 'dceg.'
        When method get
        Then status 200
        And match each $.results[*].url contains 'https://dceg.cancer.gov/'

    Scenario Outline: different results set sizes are requested without affecting the reported total number of matches
        for size: <size>

        Given path 'Search', 'cgov', 'en', 'cancer'
        And param size = size
        When method get
        Then status 200
        And match $.totalResults == 6889

        Examples:
            | size  |
            | 0     |
            | 10    |
            | 100   |
            | 1000  |
            | 10000 |

    # Suppressed until https://github.com/NCIOCPL/sitewide-search-api/issues/43 is fixed.
    # Uncomment when it is, adjusting the expected error string if necessary.
    # Scenario: an unsupported collection is provided
    #     Given path 'Search', 'chicken', 'en', 'cervical'
    #     When method get
    #     Then status 400
    #     And match $.Message == 'Not a supported collection'

    Scenario: an unsupported language is specified
        Given path 'Search', 'cgov', 'fr', 'cancer'
        When method get
        Then status 400
        And match $.Message == 'Not a valid language code.'
