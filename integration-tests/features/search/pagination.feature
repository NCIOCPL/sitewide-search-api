Feature: Search Pagination

    Background:
        * url apiHost

    Scenario Outline: Changing the size parameter manipulates the number of results returned without affecting the reported total number of matches
        for size: <size>

        Given path 'Search', 'cgov', 'en', 'cancer'
        And param size = size
        When method get
        Then status 200
        And def totalResultsInData = 6894
        And def numResultsExpected = (<size> > totalResultsInData ? totalResultsInData : <size>)
        And match $.results == '#[numResultsExpected]'
        And match $.totalResults == totalResultsInData

        Examples:
            | size  |
            | 1     |
            | 10    |
            | 100   |
            | 1000  |
            | 10000 |

    Scenario Outline: The Nth result of one search should match the first response of a subsequent search with from=N
        for result: <result>, size: <size>

        Given path 'Search', 'cgov', 'en', 'cancer'
        And param size = size
        When method get
        Then status 200
        And def firstResult = $.results[<result>].url
        #And print '1st result :' + firstResult

        Given path 'Search', 'cgov', 'en', 'cancer'
        And param from = result
        And param size = size
        When method get
        Then status 200
        And def secondResult = $.results[0].url
        #And print '2nd result: ' + secondResult
        And match firstResult == secondResult

        Examples:
            | result  | size  |
            | 4       | 10    |
            | 97      | 100   |
            | 172     | 250   |
            | 3492    | 4000  |