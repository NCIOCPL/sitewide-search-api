Feature: Autosuggest Size

    Background:
        * url apiHost

    Scenario Outline: Number of returned results should be equal to the size specified
        for size: <size>

        Given path 'Autosuggest', 'cgov', 'en', 'can'
        And param size = size
        When method get
        Then status 200
        And match $.results == '#[size]'

        Examples:
            | size |
            | 1    |
            | 2    |
            | 3    |
            | 20   |
            | 200  |