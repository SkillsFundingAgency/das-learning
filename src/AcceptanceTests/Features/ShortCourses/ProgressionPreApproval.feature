Feature: Short Course Progression before approval
    Covers FLP-1858. SLD can bundle multiple learnings for a learner in a single POST prior to any approval that Academic Year.
    This feature covers checks to ensure we handle each as discrete learnings, creating new ones, updating existing ones, and
    allowing removal-by-omission and reinstatment. This includes reinstatement via POST of all approved learnings.

Scenario Outline: Progression across multiple POSTs while first course remains unapproved
    Given an unapproved short course exists for course SC-001
    And the learner has <EndType> SC-001
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
    Given an unapproved short course exists for course SC-001
    And the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    When SLD claims the 30% milestone on SC-002
    Then SC-002 has the 30% milestone
    And SC-001 has no 30% milestone

Scenario: Removal by omission - omitting the second course from a later POST removes it
    Given an unapproved short course exists for course SC-001
    And the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    When SLD omits SC-002 from the next POST
    Then SC-002 is removed
    And SC-001 is not removed

Scenario: Reinstatement via POST - re-including an omitted course reinstates it
    Given an unapproved short course exists for course SC-001
    And the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    And SLD has omitted SC-002 from the next POST
    When SLD includes SC-002 in the next POST
    Then SC-002 is not removed
    And SC-001 is not removed

Scenario: DELETE removes all of the learner's unapproved episodes for that provider
    Given an unapproved short course exists for course SC-001
    And the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    When SLD removes all learning for the learner
    Then SC-001 is removed
    And SC-002 is removed

Scenario: Reinstatement via POST after a full DELETE
    Given an unapproved short course exists for course SC-001
    And the learner has withdrawn SC-001
    And SLD has POSTed new course SC-002
    And SLD removes all learning for the learner
    When SLD POSTs ended course SC-001 and new course SC-002
    Then SC-001 is not removed
    And SC-002 is not removed

#Toggle-off tests

Scenario: ShortCourseProgression feature off: bundled POST for new course is ignored
    Given an unapproved short course exists for course SC-001
    And the ShortCourseProgression feature is toggled off
    And the learner has completed SC-001
    When SLD POSTs ended course SC-001 and new course SC-002
    Then 1 short course learnings exist for the learner
    And a learning exists for course SC-001

Scenario: ShortCourseProgression feature off: second new course in a single bundled POST for a brand new learner is gated
    Given the ShortCourseProgression feature is toggled off
    When SLD POSTs a brand new learner with courses SC-003 and SC-004
    Then 1 short course learnings exist for the learner
    And a learning exists for course SC-003