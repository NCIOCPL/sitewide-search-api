Feature: Search Site

    Background:
        * url apiHost

    Scenario Outline: Site is used to restrict pages to one specific microsite
        for site: <site>

        Given path 'Search', 'doc', 'en', 'cancer'
        And param size = 1000
        And param site = site
        When method get
        Then status 200
        And match $.totalResults != 0
        And match each $.results[*].url contains expected

        # some sites commented out because nothing in the test data
        Examples:
            | site                                      | expected                                          |
            | dceg.cancer.gov                           | https://dceg.cancer.gov/                          |
            | deainfo.nci.nih.gov                       | https://deainfo.nci.nih.gov/                      |
            | www.cancer.gov/pediatric-adult-rare-tumor | https://www.cancer.gov/pediatric-adult-rare-tumor |
        #    | www.cancer.gov/connect-prevention-study   | https://www.cancer.gov/connect-prevention-study   |
            | www.cancer.gov/nano                       | https://www.cancer.gov/nano                       |
            | www.cancer.gov/rare-brain-spine-tumor     | https://www.cancer.gov/rare-brain-spine-tumor     |
        #    | www.cancer.gov/physics                    | https://www.cancer.gov/physics                    |

