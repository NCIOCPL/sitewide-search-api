Feature: Search Pagination

    Background:
        * url apiHost

    Scenario Outline: Changing the size parameter manipulates the number of results returned without affecting the reported total number of matches
        for size: <size>
    
        Given path 'Search', 'cgov', 'en', '<term>'
        And param size = size
        When method get
        Then status 200
    
        Examples:
            | size  | term         |
            | 10    | liver        |
            | 100   | breast       |
            | 1000  | cancer       |
            | 10    | nci          |
            | 100   | president    |
            | 1000  | cervica      |
            | 10    | chemo        |
            | 100   | chemotherap  |
            | 10    | ethylene     |
            | 100   | glioblastoma |
            | 1000  | hpv          |
            | 10    | leukemia     |
            | 100   | onions       |
            | 1000  | melanoma     |
            | 10    | chernobyl    |
