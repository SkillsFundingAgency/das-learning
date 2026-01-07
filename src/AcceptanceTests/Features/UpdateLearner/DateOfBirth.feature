Feature: DateOfBirth

Tests for DateOfBirth under UpdateLearner

Scenario: Learner date of birth is updated
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property    | Value         |
        | DateOfBirth | 2000-05-15    |
    When the update request is sent
    Then the Date of Birth for the Learning is set to 2000-05-15
    And the following changes are returned
        | Change             |
        | DateOfBirthChanged |
    And the learning history is maintained
