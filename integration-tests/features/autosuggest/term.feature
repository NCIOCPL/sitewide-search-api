Feature: Autosuggest Term

    Background:
        * url apiHost

    Scenario: empty sequence of characters is submitted
        Given path 'Autosuggest', 'cgov', 'en', ''
        When method get
        Then status 400
        And match $.Message == 'You must supply a search term'