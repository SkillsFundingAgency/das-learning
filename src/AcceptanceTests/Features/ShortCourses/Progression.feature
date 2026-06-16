Feature: Short Course Progression
    Covers FLP-1894. When a learner ends an approved short course and starts a new
    course with the same provider, a separate Learning is created for the new course.

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
