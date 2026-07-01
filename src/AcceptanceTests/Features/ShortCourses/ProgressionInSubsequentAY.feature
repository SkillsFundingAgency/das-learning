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

Scenario Outline: Prior-AY <EndType> course is unaffected when the new-AY course is updated via PUT
    Given a short course SC-001 was <EndType> by the learner in a prior academic year
    And SLD has POSTed a new course SC-002 in the subsequent academic year
    When SLD PUTs the 30% milestone for SC-002 in the subsequent academic year
    Then SC-001 is not removed

    Examples:
        | EndType   |
        | completed |
        | withdrawn |

Scenario Outline: Prior-AY <EndType> course is unaffected when the new-AY course is DELETEd
    Given a short course SC-001 was <EndType> by the learner in a prior academic year
    And SLD has POSTed a new course SC-002 in the subsequent academic year
    When SLD DELETEs the learner in the subsequent academic year
    Then SC-001 is not removed

    Examples:
        | EndType   |
        | completed |
        | withdrawn |

Scenario: A course with no end date spanning the AY boundary can be PUT updated in the subsequent AY
    Given a short course SC-001 started in a prior academic year with no completion or withdrawal date
    When SLD PUTs the 30% milestone for SC-001 in the subsequent academic year
    Then SC-001 has the 30% milestone
    And SC-001 is not removed

Scenario: A course with no end date spanning the AY boundary is removed when DELETEd in the subsequent AY
    Given a short course SC-001 started in a prior academic year with no completion or withdrawal date
    When SLD DELETEs the learner in the subsequent academic year
    Then SC-001 is removed
