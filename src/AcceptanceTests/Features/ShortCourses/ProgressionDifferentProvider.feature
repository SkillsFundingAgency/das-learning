Feature: Short Course Progression with a Different Provider
    Covers FLP-1861, FLP-1862, FLP-1863 and FLP-1878. A learner completes or withdraws from a short course with Provider A,
    then begins a different short course with Provider B in the same or a subsequent academic year.

Scenario: Learner completes a course with Provider A then begins a different course with Provider B (unapproved earnings)
    Given SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     1 | SC-001     |
    And short course is approved
    And SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode | CompletionDate |
        | 54321 |     1 | SC-001     | 2025-06-01     |
    When SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     2 | SC-002     |
    Then the response from the create short course endpoint is 200
    And 2 short course learnings exist for the learner
    And a learning exists for course SC-001 with Ukprn 1
    And a learning exists for course SC-002 with Ukprn 2

Scenario: Learner withdraws from a course with Provider A then begins a different course with Provider B (unapproved earnings)
    Given SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     1 | SC-001     |
    And short course is approved
    And SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode | WithdrawalDate |
        | 54321 |     1 | SC-001     | 2025-06-01     |
    When SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     2 | SC-002     |
    Then the response from the create short course endpoint is 200
    And 2 short course learnings exist for the learner
    And a learning exists for course SC-001 with Ukprn 1
    And a learning exists for course SC-002 with Ukprn 2

Scenario: Approving the second course (different provider) does not affect the first
    Given SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     1 | SC-001     |
    And short course SC-001 is approved
    And SLD call the create short course endpoint with the following information
        | Uln   | Ukprn | CourseCode |
        | 54321 |     2 | SC-002     |
    When short course SC-002 is approved
    Then SC-002 is approved
    And SC-001 remains approved and unchanged

Scenario: Recording the 30% milestone on the second course (different provider) does not affect the first
    Given an approved short course exists for course SC-001 with Provider 1
    And an approved short course exists for course SC-002 with Provider 2
    When Provider 2 records the 30% milestone for SC-002
    Then SC-002 has the 30% milestone
    And SC-001 has no 30% milestone

Scenario: Recording the completion date on the second course (different provider) does not affect the first
    Given an approved short course exists for course SC-001 with Provider 1
    And an approved short course exists for course SC-002 with Provider 2
    When Provider 2 records the completion date for SC-002
    Then SC-002 has a completion date
    And SC-001 has no completion date

Scenario Outline: Learner <EndType> a course with Provider A then begins a different course with Provider B in the subsequent academic year (unapproved earnings)
    Given a short course SC-001 was <EndType> by the learner in a prior academic year
    When SLD POSTs a new course SC-002 with Provider 2 in the subsequent academic year
    Then 2 short course learnings exist for the learner
    And a learning exists for course SC-001
    And a learning exists for course SC-002
    And SC-001 is not removed

    Examples:
        | EndType   |
        | completed |
        | withdrawn |
