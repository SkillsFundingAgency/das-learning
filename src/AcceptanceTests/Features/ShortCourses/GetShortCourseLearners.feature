Feature: GetShortCourseLearners

Scenario: Include learners that were active in the relevant academic year
	Given SLD has informed the system of the following short courses
		| FirstName | LastName   | Uln   | StartDate  | ExpectedEndDate | IsApproved |
		| Bob       | FullAY     | 54321 | 2024-08-01 | 2025-07-31      | True       |
		| Tracey    | StartInAY  | 54322 | 2024-12-01 | 2025-10-01      | True       |
		| Seth      | FinishInAY | 54323 | 2024-01-01 | 2024-12-01      | True       |
		| Lionel    | NextAY     | 54324 | 2025-09-01 | 2026-06-30      | True       |
		| Samantha  | PreviousAY | 54325 | 2023-09-01 | 2024-06-30      | True       |
		| Rudolph   | Unapproved | 54326 | 2024-08-01 | 2025-07-31      | False      |
	When SLD requests the list of short courses for academic year 2425
	Then short courses are returned for the following Ulns
		| Uln   |
		| 54321 |
		| 54322 |
		| 54323 |