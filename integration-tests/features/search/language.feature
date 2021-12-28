Feature: Search Language

    Background:
        * url apiHost

    Scenario Outline: Language is used to restrict pages to one specific language
        for language: <language>

        Given path 'Search', 'cgov', language, 'cancer'
        And param size = 1000
        When method get
        Then status 200
        And match $.totalResults != 0
        And match each $.results[*].url <operator> expected

        Examples:
            | language | operator | expected                        |
            | es       | contains | https://www.cancer.gov/espanol  |
            | en       | !contains | https://www.cancer.gov/espanol |

    Scenario: an unsupported language is specified
            Given path 'Search', 'cgov', 'fr', 'cancer'
            When method get
            Then status 400
            And match $.Message == 'Not a valid language code.'