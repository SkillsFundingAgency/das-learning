Feature: PersonalDetails

Tests updates of personal details

Scenario: Learner First Name is updated
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property  | Value    |
        | FirstName | Dave     |
        | LastName  | Davidson |
    When the update request is sent
	Then the Learners name is updated to Dave Davidson
    And the following changes are returned
        | Change             |
        | PersonalDetails |
    And the learning history is maintained
	Then the PersonalDetailsChangedEvent is emitted

