Feature: Search Collection

    Background:
        * url apiHost

    # Suppressed until https://github.com/NCIOCPL/sitewide-search-api/issues/43 is fixed.
    # Uncomment when it is, adjusting the expected error string if necessary.
    #Scenario: an unsupported collection is provided
    #    Given path 'Search', 'chicken', 'en', 'cervical'
    #    When method get
    #    Then status 400
    #    And match $.Message == 'Not a supported collection'