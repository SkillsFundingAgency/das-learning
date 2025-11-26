Feature: BreaksInLearning

Tests for BreaksInLearning under UpdateLearner

Scenario: BreaksInLearning details are added
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property           | Value                                          |
        | BreaksInLearning   | startDate:currentAY-09-25 endDate:nextAY-07-31 |
    When the update request is sent
    Then the following BreaksInLearning details are stored
        | StartDate       | EndDate      |
        | currentAY-09-25 | nextAY-07-31 |
    And the following changes are returned
        | Change                 |
        | BreaksInLearningUpdated |


Scenario: BreaksInLearning details are added then removed
    Given There is an apprenticeship with the following details
        | StartDate       | EndDate      | TrainingPrice | EpaPrice |
        | currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
    And an update request has the following data
        | Property           | Value                                          |
        | BreaksInLearning   | startDate:currentAY-09-25 endDate:nextAY-07-31 |
    And the update request is sent
    And an update request has the following data
        | Property           | Value |
        | BreaksInLearning   |       |
    When the update request is sent
    Then the following BreaksInLearning details are stored
        | StartDate | EndDate |
    And the following changes are returned
        | Change                 |
        | BreaksInLearningUpdated |
