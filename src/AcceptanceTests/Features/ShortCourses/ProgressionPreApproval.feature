Feature: Short Course Progression before approval
    Covers FLP-1858. SLD can bundle multiple learnings for a learner in a single POST prior to any approval that Academic Year.
    This feature covers checks to ensure we handle each as discrete learnings, creating new ones, updating existing ones, and
    allowing removal-by-omission and reinstatment. This includes reinstatement via POST of all approved learnings.

Background:
    Given an unapproved short course exists for course SC-001

Scenario Outline: Progression across multiple POSTs while first course remains unapproved
    Given the learner has <EndType> SC-001
    When SLD POSTs ended course SC-001 and new course SC-002
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002
    And SC-002 is unapproved

    Examples:
        | EndType   |
        | completed |
        | withdrawn |

Scenario: Single POST with both one already-ended course and progression creates both learnings
    When SLD POSTs a brand new learner with completed course SC-001 and new course SC-002
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002
    And SC-002 is unapproved

Scenario: Claiming the 30% milestone on the second course does not affect the first (POST-created)
    Given the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    When SLD claims the 30% milestone on SC-002
    Then SC-002 has the 30% milestone
    And SC-001 has no 30% milestone

#Toggle-off tests

Scenario: ShortCourseProgression feature off: bundled POST for new course is ignored
    Given the ShortCourseProgression feature is toggled off
    And the learner has completed SC-001
    When SLD POSTs ended course SC-001 and new course SC-002
    Then 1 short course learnings exist for the learner
    And a learning exists for course SC-001

Scenario: ShortCourseProgression feature off: second new course in a single bundled POST for a brand new learner is gated
    Given the ShortCourseProgression feature is toggled off
    When SLD POSTs a brand new learner with courses SC-003 and SC-004
    Then 1 short course learnings exist for the learner
    And a learning exists for course SC-003