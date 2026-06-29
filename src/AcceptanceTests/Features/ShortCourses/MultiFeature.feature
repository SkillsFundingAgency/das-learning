Feature: Multi-Feature Interaction - Progression then Change of Provider
    Covers the interaction between Progression and Change of Provider where a learner is on a Short Course
    with a Provider (A) but another Provider (B) submits a new course for the same learner, but withdraws early because
    the learner is going to Change Provider back to Provider A for the rest of this new course.   

Background:
    Given an approved short course exists for course SC-001
    And Provider B has POSTed new course SC-002
    And Provider B has withdrawn SC-002

Scenario Outline: Provider A PUTs a course currently owned by Provider B
    Given the learner has <EndType> SC-001
    When Provider A PUTs course SC-002
    Then SC-002 has 2 episodes
    And SC-002's episodes belong to a single Learning
    And SC-001 has 1 episode

    Examples:
        | EndType   |
        | completed |
        | withdrawn |

# Toggle-off tests
# Progression will ship first while Change of Provider is toggled off. This test ensures that the Change of Provider functionality
# is gated behind the ShortCourseChangeOfProvider feature flag, and the added complication of Progression does not break the gating.

Scenario: Change of Provider functionality is silently disabled when that feature is toggled off
    Given the learner has completed SC-001
    And the ShortCourseChangeOfProvider feature is toggled off
    When Provider A PUTs course SC-002
    Then SC-002 has 1 episode
    And SC-002's only episode belongs to Provider B
