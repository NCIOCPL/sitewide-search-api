# Given our sample data we might have autosuggests with no matching terms
# So this test is only partially valid but could be expanded in the future with better data
# Or used on 'live' data to validate autosuggest

Feature: Autosuggest Search Results

    Background:
        * url apiHost

    Scenario Outline: All returned autosuggest terms should have matching search results
        for term: <term>

        Given path 'Autosuggest', 'cgov', 'en', term
        And param size = 20
        When method get
        Then status 200
        And call read('../search/simple-term-search.feature') $.results

        Examples:
            | term    |
            | can     |
            | lung    |
            | bre     |
            | pro     |
            | leuk    |
            | mel     |
            | pan     |
            | colo    |