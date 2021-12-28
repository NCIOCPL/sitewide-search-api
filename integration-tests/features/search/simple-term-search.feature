@ignore
Feature: Simple Search Term
    
    # Designed for call by other features.

    Background:
        * url apiHost

    Scenario: Simple search with term returns results

        # A newer Karate version will respect the @ignore special tag
        # In which case the line below will be unnecessary because this would only be called with term provided
        Given def searchterm = (typeof term == 'undefined' ? 'cancer' : term)
        And path 'Search','cgov','en',searchterm
        And param size = 5
        When method get
        And status 200
        And match $.totalResults != 0
        
