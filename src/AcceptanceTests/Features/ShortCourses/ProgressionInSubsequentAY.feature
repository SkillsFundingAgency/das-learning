Feature: Short Course Progression in a subsequent academic year
    Covers FLP-1875. When a learner ends a short course in one academic year and starts a different
    short course with the same provider in the next academic year, the prior-year learning must not
    be removed.

Scenario Outline: Prior-AY <EndType> course is unaffected when a new course is POSTed in the subsequent AY
    Given a short course SC-001 was <EndType> by the learner in a prior academic year
    When SLD POSTs a new course SC-002 in the subsequent academic year
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002
    And SC-001 is not removed

    Examples:
        | EndType   |
        | completed |
        | withdrawn |
