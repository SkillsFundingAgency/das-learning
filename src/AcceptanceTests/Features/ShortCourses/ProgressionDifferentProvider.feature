Feature: Short Course Progression with a Different Provider
    Covers FLP-1861 and FLP-1862. A learner completes or withdraws from a short course with Provider A,
    then begins a different short course with Provider B in the same academic year.
    Provider B has no prior history for this learner so the new course always arrives as a POST.

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
