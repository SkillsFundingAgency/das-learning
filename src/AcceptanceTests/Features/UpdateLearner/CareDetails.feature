Feature: CareDetails

Tests that care detail changes are recognised and persisted

Scenario: Learner care details are updated
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property    | Value                                                      |
        | CareDetails | HasEHCP:true IsCareLeaver:true CareLeaverEmployerConsentGiven:True |
    When the update request is sent
    Then the Care details for the Learning is
		| HasEHCP | IsCareLeaver | CareLeaverEmployerConsentGiven |
		| true    | true         | True                   |
    And the following changes are returned
        | Change |
        | Care   |

Scenario: Learner care details are updated then amended
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property    | Value                                                      |
        | CareDetails | HasEHCP:true IsCareLeaver:true CareLeaverEmployerConsentGiven:True |
    And the update request is sent
    And the Care details for the Learning is
		| HasEHCP | IsCareLeaver | CareLeaverEmployerConsentGiven |
		| true    | true         | True                   |
    And an update request has the following data
        | Property    | Value                                                       |
        | CareDetails | HasEHCP:true IsCareLeaver:true CareLeaverEmployerConsentGiven:False |
    When the update request is sent
    Then the Care details for the Learning is
		| HasEHCP | IsCareLeaver | CareLeaverEmployerConsentGiven |
		| true    | true         | False                  |
    And the following changes are returned
        | Change |
        | Care   |