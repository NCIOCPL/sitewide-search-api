Feature: Search Collection

    Background:
        * url apiHost

    Scenario: an unsupported collection is provided
        Given path 'Search', 'chicken', 'en', 'cervical'
        When method get
        Then status 400
        And match $.Message == 'Not a valid collection.'
