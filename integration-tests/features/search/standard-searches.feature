Feature: Standard Searches

    Background:
        * url apiHost

    Scenario Outline: Pages should match the expected search results
        for collection: <collection>, lang: <lang>, term: <term>, from: <from>, size: <size>, site: <site>

        Given path 'Search', collection, lang, term
        And param from = from
        And param size = size
        And param site = site
        When method get
        Then status 200
        And def filename = (equivalent ? equivalent : '<collection>-<lang>-' + term.split(' ').join('+') + '-<from>-<size>-' + site.split('/').join('+'))
        And match response == read('standard-searches.expected/' + filename + '.json')

        # See consideration in README.md before using karate.write
        #And karate.write(response, (equivalent ? 'DELETE-' : '') + 'standard-searches.expected/' + filename + '.json')

        Examples:
            | collection | lang | term                         | from | size | site                                          | equivalent                                                            |
            | cgov       | en   | liver                        | 0    | 5    | all                                           |                                                                  |
            | cgov       | en   | liver                        | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | LIVER                        | 0    | 5    | all                                           | cgov-en-liver-0-5-all                                            |
            | cgov       | en   | sezary                       | 0    | 5    | all                                           |                                                                  |
            | cgov       | en   | sézary                       | 0    | 5    | all                                           | cgov-en-sezary-0-5-all                                           |
            | cgov       | en   | Sezary                       | 0    | 5    | all                                           | cgov-en-sezary-0-5-all                                           |
            | cgov       | en   | Sézary                       | 0    | 5    | all                                           | cgov-en-sezary-0-5-all                                           |
            | cgov       | en   | liver                        | 1    | 5    | all                                           |                                                                  |
            | cgov       | en   | liver onions                 | 0    | 5    | all                                           |                                                                  |
            | cgov       | en   | liver                        |      | 5    | all                                           | cgov-en-liver-0-5-all                                            |
            | cgov       | en   | liver                        | 0    |      | all                                           | cgov-en-liver-0-10-all                                           |
            | cgov       | en   | liver                        |      |      | all                                           | cgov-en-liver-0-10-all                                           |
            | cgov       | en   | liver                        |      |      |                                               | cgov-en-liver-0-10-all                                           |
            | cgov       | en   | liver                        | 0    | 5    | dceg.cancer.gov                               | cgov-en-liver-0-5-all                                            |
            | cgov       | en   | liver                        | 0    | 5    | dada.cancer.gov                               | cgov-en-liver-0-5-all                                            |
            | cgov       | es   | liver                        | 0    | 5    | all                                           |                                                                  |
            | cgov       | es   | corazon                      | 0    | 5    | all                                           |                                                                  |
            | cgov       | es   | corazón                      | 0    | 5    | all                                           | cgov-es-corazon-0-5-all                                          |
            | doc        | en   | liver                        | 0    | 5    | all                                           |                                                                  |
            | doc        | en   | liver                        | 0    | 5    | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | liver                        | 0    | 5    | dada.cancer.gov                               |                                                                  |
            | doc        | es   | corazon                      | 0    | 5    | dceg.cancer.gov                               |                                                                  |
            | doc        | es   | corazón                      | 0    | 5    | dceg.cancer.gov                               | doc-es-corazon-0-5-dceg.cancer.gov                               |
            | doc        | en   | liver                        | 0    | 5    | deainfo.nci.nih.gov                           |                                                                  |
            | cgov       | en   | breast cancer                | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | lung cancer                  | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | prostate cancer              | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | melanoma                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | leukemia                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | cancer                       | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | cervical cancer              | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | pancreatic cancer            | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | colon cancer                 | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | ovarian cancer               | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | ethylene oxide               | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | multiple myeloma             | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | glioblastoma                 | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | skin cancer                  | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | lymphoma                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | immunotherapy                | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | chemotherapy                 | 0    | 10   | all                                           |                                                                  |
            | cgov       | en   | hpv                          | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | cancer                       | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | cancer de mama               | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | cáncer de mama               | 0    | 10   | all                                           | cgov-es-cancer+de+mama-0-10-all                                  |
            | cgov       | es   | genotipo                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | Intestino grueso             | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | leucemia                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | neoplasia                    | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | plaquetas                    | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | carcinoma                    | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | quimioterapia                | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | anticuerpo                   | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | celula                       | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | ARN                          | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | linfocito                    | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | pancreas                     | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | linfoma                      | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | cancer de pulmon             | 0    | 10   | all                                           |                                                                  |
            | cgov       | es   | cáncer de pulmón             | 0    | 10   | all                                           | cgov-es-cancer+de+pulmon-0-10-all                                |
            | doc        | en   | cancer                       | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | melanoma                     | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | nanotechnology               | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | skin cancer                  | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | tumor                        | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | melanoma                     | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | understanding nanodevices    | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | author                       | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | journey into nanotechnology  | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | nanomedicine                 | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | nanowires                    | 0    | 10   | www.cancer.gov/nano                           |                                                                  |
            | doc        | en   | cancer                       | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | intranet                     | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | breast cancer                | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | chanock                      | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | connect                      | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | jonas                        | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | melanoma                     | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | covid                        | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | Chernobyl                    | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | physical activity            | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | confluence                   | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | shiels                       | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | sarah jackson                | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | landi                        | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | trinity                      | 0    | 10   | dceg.cancer.gov                               |                                                                  |
            | doc        | en   | glioblastoma                 | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | glioblastoma multiforme      | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | astrocytoma                  | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | meningioma                   | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | glioma                       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | medulloblastoma              | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | oligodendroglioma            | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | cancer                       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | ependymoma                   | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | tumor                        | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | anaplastic astrocytoma       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | schwannoma                   | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | en   | neuroblastoma                | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor         |                                                                  |
            | doc        | es   | glioblastoma                 | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | glioblastoma multiforme      | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | astrocitoma                  | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | meningioma                   | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | glioma                       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | meduloblastoma               | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | oligodendroglioma            | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | cancer                       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | cáncer                       | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol | doc-es-cancer-0-10-www.cancer.gov+rare-brain-spine-tumor+espanol |
            | doc        | es   | ependimoma                   | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | tumor                        | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |
            | doc        | es   | neuroblastoma                | 0    | 10   | www.cancer.gov/rare-brain-spine-tumor/espanol |                                                                  |



    