Feature: UpdateShortCourse

Scenario: Update unapproved short course withdrawal date
    Given SLD has informed the system that a new short course has been created
    When SLD calls the update short course endpoint with the following information
        | WithdrawalDate | WithdrawalReasonCode |
        | 2025-01-01     | 2                    |
    Then the update short course response includes changes
        | Change         |
        | WithdrawalDate |

Scenario: Update approved short course withdrawal date
    Given SLD has informed the system that a new short course has been created
    And short course is approved
    When SLD calls the update short course endpoint with the following information
        | WithdrawalDate | WithdrawalReasonCode |
        | 2025-01-01     | 2                    |
    Then the update short course response includes changes
        | Change         |
        | WithdrawalDate |
    And the correct learning withdrawn event is emitted

Scenario: Update short course completion date
    Given SLD has informed the system that a new short course has been created
    When SLD calls the update short course endpoint with the following information
        | CompletionDate |
        | 2025-01-01     |
    Then the update short course response includes changes
        | Change         |
        | CompletionDate |

Scenario: Update short course milestones
    Given SLD has informed the system that a new short course has been created
    When SLD calls the update short course endpoint with the following information
        | Milestone        |
        | LearningComplete |
    Then the update short course response includes changes
        | Change    |
        | Milestone |

Scenario: Update short course with no changes
    Given SLD has informed the system that a new short course has been created
    When SLD calls the update short course endpoint with no changes
    Then the update short course response includes no changes

Scenario: Update short course response includes completion date
    Given SLD has informed the system that a new short course has been created
    And short course is approved
    When SLD calls the update short course endpoint with the following information
        | CompletionDate |
        | 2025-06-01     |
    Then the update short course response has a completion date of 2025-06-01

Scenario: Update short course response includes learner and episode details
    Given SLD has informed the system that a new short course has been created
    And short course is approved
    When SLD calls the update short course endpoint with no changes
    Then the update short course response includes the following learner details
        | Uln    | FirstName | LastName   |
        | 123213 | Frank     | Frankinson |
    And the update short course response includes the following episode details
        | Ukprn    | EmployerAccountId | CourseCode | CourseType  | IsApproved | Price | AgeAtStart |
        | 10005001 | 99999999          | SC-ART1    | ShortCourse | true       | 1000  | 24         |

Scenario: Update short course includes ForceEarningsSync when set and excludes it when unset
    Given SLD has informed the system that a new short course has been created
    And the short course episode ForceEarningsSync is set to True
    When SLD calls the update short course endpoint with no changes
    Then the update short course response includes changes
        | Change            |
        | ForceEarningsSync |
    And the short course episode ForceEarningsSync is set to False
    When SLD calls the update short course endpoint with no changes
    Then the update short course response includes no changes
