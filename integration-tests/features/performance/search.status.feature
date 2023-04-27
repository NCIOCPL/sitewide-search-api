Feature: Search Status

    Background:
        * url apiHost

    Scenario: Confirm that the API and the Search path are in a working status
        Given path 'Search', 'status'
        When method get
        Then status 200
        And match response == 'alive!'
