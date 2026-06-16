Feature: Short Course Progression
    Covers FLP-1894. When a learner ends an approved short course and starts a new
    course with the same provider, a separate Learning is created for the new course.
    Subsequent lifecycle events on either course should not affect the other.

Background:
    Given an approved short course exists for course SC-001

Scenario Outline: Progression after an ended course creates a new Learning
    Given the learner has <EndType> SC-001
    When SLD submits a progression PUT for new course SC-002
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002

    Examples:
        | EndType   |
        | completed |
        | withdrawn |

Scenario: Simultaneous completion and progression creates a new Learning
    When SLD sends a progression PUT with SC-001 completed and new course SC-002
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002

Scenario: Claiming the 30% milestone on SC-002 does not affect SC-001
    Given the learner has completed SC-001
    And a progression PUT has added new course SC-002
    When SLD claims the 30% milestone on SC-002
    Then SC-002 has the 30% milestone
    And SC-001 has no milestones

Scenario: Withdrawing SC-002 does not affect SC-001
    Given the learner has completed SC-001
    And a progression PUT has added new course SC-002
    When SLD withdraws SC-002
    Then SC-002 is withdrawn
    And SC-001 is not withdrawn

Scenario: Updating the completion date of SC-001 does not affect SC-002
    Given the learner has completed SC-001
    And a progression PUT has added new course SC-002
    When SLD updates the completion date of SC-001
    Then SC-001 has a completion date
    And SC-002 has no completion date

Scenario: Removing SC-002 does not affect SC-001
    Given the learner has completed SC-001
    And a progression PUT has added new course SC-002
    When SLD omits SC-002 from the next PUT
    Then SC-002 is removed
    And SC-001 is not removed

Scenario: Reinstating SC-002 does not affect SC-001
    Given the learner has completed SC-001
    And a progression PUT has added new course SC-002
    And SLD has omitted SC-002 from the next PUT
    When SLD includes SC-002 in the next PUT
    Then SC-002 is not removed
    And SC-001 is not removed
