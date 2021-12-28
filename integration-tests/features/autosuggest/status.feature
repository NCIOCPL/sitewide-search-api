Feature: Autosuggest Status

    Background:
        * url apiHost

    Scenario: Confirm that the API and the Autosuggest path are in a working status
        Given path 'Autosuggest', 'status'
        When method get
        Then status 200
        And match response == 'alive!'