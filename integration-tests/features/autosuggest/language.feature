Feature: Autosuggest Language

    Background:
        * url apiHost

    # Since the results don't return the specified language and/or path
    # there is no easy way to validate that the results are in the chosen language
    # so we leave that to the standard search terms feature

    Scenario: An unsupported language is specified
        Given path 'Autosuggest', 'cgov', 'de', 'cancer'
        When method get
        Then status 400
        And match $.Message == 'Not a valid language code.'
