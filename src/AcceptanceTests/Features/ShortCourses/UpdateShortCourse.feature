Feature: UpdateShortCourse

Scenario: Update short course withdrawal date
    Given SLD has informed the system that a new short course has been created
    When SLD calls the update short course endpoint with the following information
        | WithdrawalDate |
        | 2025-01-01     |
    Then the update short course response includes changes
        | Change         |
        | WithdrawalDate |

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
