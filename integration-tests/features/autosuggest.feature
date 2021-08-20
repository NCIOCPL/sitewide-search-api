Feature: Autosuggest

    Background:
        * url apiHost

    Scenario: we need to confirm that the feature is available
        Given path 'Autosuggest', 'status'
        When method get
        Then status 200
        And match response == 'alive!'

    Scenario Outline: we want valid terms most closely matching what the user has typed so far
        for lang: <lang>, term: <term>, size: <size>

        Given path 'Autosuggest', 'cgov', lang, term
        And param size = size
        When method get
        Then status 200
        And match response == read('autosuggest-' + expected + '.json')

        Examples:
            # When https://github.com/NCIOCPL/sitewide-search-api/issues/50 has been fixed
            # add tests for overriding size default.
            | lang | term    | size | expected      |
            | en   | a       | 10   | en-a-10       |
            | en   | a       |      | en-a-10       |
            | en   | ab      | 10   | en-ab-10      |
            | en   | abc     | 10   | en-abc-10     |
            | en   | A       | 10   | en-a-10       |
            | en   | ä       | 10   | en-a-10       |
            | en   | Ä       | 10   | en-a-10       |
            | en   | lung    | 10   | en-lung-10    |
            | en   | lung ca | 10   | en-lung_ca-10 |
            | en   | toenail | 10   | no-hits       |
            | en   | [       | 10   | en-bracket-10 |
            | en   | (       | 10   | en-paren-10   |
            | en   | (v      | 10   | en-parenv-10  |
            | en   | -       | 10   | no-hits       |
            | en   | =       | 10   | no-hits       |
            | en   | +       | 10   | no-hits       |
            | en   | *       | 10   | no-hits       |
            | en   | {       | 10   | no-hits       |
            | en   | "       | 10   | no-hits       |
            | en   | '       | 10   | no-hits       |
            | en   | @       | 10   | no-hits       |
            | en   | 1       | 10   | en-1-10       |
            | en   | 12      | 10   | en-12-10      |
            | en   | 123     | 10   | en-123-10     |
            | en   | i 123   | 10   | en-i_123-10   |
            | en   | I 123   | 10   | en-i_123-10   |
            | es   | a       | 10   | es-a-10       |
            | es   | ab      | 10   | es-ab-10      |
            | es   | abc     | 10   | es-abc-10     |
            | es   | A       | 10   | es-a-10       |
            | es   | ä       | 10   | es-a-10       |
            | es   | Ä       | 10   | es-a-10       |
            | es   | lung    | 10   | es-lung-10    |
            | es   | pulmon  | 10   | es-pulmon-10  |
            | es   | pulmón  | 10   | es-pulmon-10  |
            | es   | duende  | 10   | no-hits       |
            | es   | [       | 10   | es-bracket-10 |
            | es   | (       | 10   | es-paren-10   |
            | es   | (v      | 10   | es-parenv-10  |
            | es   | -       | 10   | no-hits       |
            | es   | =       | 10   | no-hits       |
            | es   | +       | 10   | no-hits       |
            | es   | *       | 10   | no-hits       |
            | es   | {       | 10   | no-hits       |
            | es   | "       | 10   | no-hits       |
            | es   | '       | 10   | no-hits       |
            | es   | @       | 10   | no-hits       |
            | es   | 1       | 10   | es-1-10       |
            | es   | 12      | 10   | es-12-10      |
            | es   | 123     | 10   | es-123-10     |
            | es   | i 123   | 10   | es-i_123-10   |
            | es   | I 123   | 10   | es-i_123-10   |

    # Suppressed until https://github.com/NCIOCPL/sitewide-search-api/issues/43 is fixed.
    # Uncomment when it is, adjusting the expected error string if necessary.
    # Scenario: an unsupported collection is provided
    #     Given path 'Autosuggest', 'turkey', 'en', 'cervical'
    #     When method get
    #     Then status 400
    #     And match $.Message == 'Not a supported collection'

    Scenario: empty sequence of characters is submitted
        Given path 'Autosuggest', 'cgov', 'en', ''
        When method get
        Then status 400
        And match $.Message == 'You must supply a search term'

    Scenario: an unsupported language is specified
        Given path 'Autosuggest', 'cgov', 'de', 'cancer'
        When method get
        Then status 400
        And match $.Message == 'Not a valid language code.'
