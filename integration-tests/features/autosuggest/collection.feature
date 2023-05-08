Feature: Autosuggest Collection

    Background:
        * url apiHost

    Scenario: an unsupported collection is provided
        Given path 'Autosuggest', 'turkey', 'en', 'cervical'
        When method get
        Then status 400
        And match $.Message == 'Not a valid collection.'
