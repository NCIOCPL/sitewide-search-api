Feature: Standard Autosuggests

    Background:
        * url apiHost

    Scenario Outline: we want valid terms most closely matching what the user has typed so far
        for lang: <lang>, term: <term>, size: <size>

        Given path 'Autosuggest', 'cgov', lang, term
        And param size = size
        When method get
        Then status 200
        And match response == read('standard-autosuggests.expected/autosuggest-' + filename + '.json')

        # See consideration in README.md before using karate.write
        #And karate.write(response, (equivalent ? 'DELETE-' : '') + 'standard-autosuggests.expected/autosuggest-' + filename + '.json')

        Examples:
            | lang | term    | size | filename      | equivalent |
            | en   | a       | 10   | en-a-10       |            |
            | en   | a       |      | en-a-10       |            |
            | en   | ab      | 10   | en-ab-10      |            |
            | en   | abc     | 10   | en-abc-10     |            |
            | en   | A       | 10   | en-a-10       | yes        |
            | en   | ä       | 10   | en-a-10       | yes        |
            | en   | Ä       | 10   | en-a-10       | yes        |
            | en   | lung    | 10   | en-lung-10    |            |
            | en   | lung ca | 10   | en-lung_ca-10 |            |
            | en   | toenail | 10   | no-hits       | yes        |
            | en   | [       | 10   | en-bracket-10 |            |
            | en   | (       | 10   | en-paren-10   |            |
            | en   | (v      | 10   | en-parenv-10  |            |
            | en   | -       | 10   | no-hits       | yes        |
            | en   | =       | 10   | no-hits       | yes        |
            | en   | +       | 10   | no-hits       | yes        |
            | en   | *       | 10   | no-hits       | yes        |
            | en   | {       | 10   | no-hits       | yes        |
            | en   | "       | 10   | no-hits       | yes        |
            | en   | '       | 10   | no-hits       | yes        |
            | en   | @       | 10   | no-hits       | yes        |
            | en   | 1       | 10   | en-1-10       |            |
            | en   | 12      | 10   | en-12-10      |            |
            | en   | 123     | 10   | en-123-10     |            |
            | en   | i 123   | 10   | en-i_123-10   |            |
            | en   | I 123   | 10   | en-i_123-10   |            |
            | es   | a       | 10   | es-a-10       |            |
            | es   | ab      | 10   | es-ab-10      |            |
            | es   | abc     | 10   | es-abc-10     |            |
            | es   | A       | 10   | es-a-10       |            |
            | es   | ä       | 10   | es-a-10       |            |
            | es   | Ä       | 10   | es-a-10       |            |
            | es   | lung    | 10   | es-lung-10    |            |
            | es   | pulmon  | 10   | es-pulmon-10  |            |
            | es   | pulmón  | 10   | es-pulmon-10  | yes        |
            | es   | duende  | 10   | no-hits       | yes        |
            | es   | [       | 10   | es-bracket-10 |            |
            | es   | (       | 10   | es-paren-10   |            |
            | es   | (v      | 10   | es-parenv-10  |            |
            | es   | -       | 10   | no-hits       | yes        |
            | es   | =       | 10   | no-hits       | yes        |
            | es   | +       | 10   | no-hits       | yes        |
            | es   | *       | 10   | no-hits       | yes        |
            | es   | {       | 10   | no-hits       | yes        |
            | es   | "       | 10   | no-hits       | yes        |
            | es   | '       | 10   | no-hits       | yes        |
            | es   | @       | 10   | no-hits       | yes        |
            | es   | 1       | 10   | es-1-10       |            |
            | es   | 12      | 10   | es-12-10      |            |
            | es   | 123     | 10   | es-123-10     |            |
            | es   | i 123   | 10   | es-i_123-10   |            |
            | es   | I 123   | 10   | es-i_123-10   |            |