Feature: Search results are returned, regardless of whether a given site uses one or multiple instances of the description metatag.

    Background:
        * url apiHost

    Scenario: Some results have multiple metatag.description values

        # EDRN biography pages typically have multiple description metatags. Several such results
        # appear in the regression-data.jsonl set and appear in this query.
        Given path 'Search', 'cgov', 'en', 'james nolan early detection research network'
        And param size = 20
        When method get
        Then status 200
        And match response == read('metatag-description-multiple.json')

    Scenario: Some results have exactly one metatag.description value.

        # The page https://www.cancer.gov/about-nci/organization/ccct/funding/ccitla (generated by the
        # digital platform) has exactly one description metatag.  It is first in this result set.
        Given path 'Search', 'cgov', 'en', 'CCCT - Funding - CCITLA - National Cancer Institute'
        And param size = 20
        When method get
        Then status 200
        And match response == read('metatag-description-single.json')
