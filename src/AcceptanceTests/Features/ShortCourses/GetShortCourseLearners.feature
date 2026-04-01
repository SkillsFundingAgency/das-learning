Feature: GetShortCourseLearners

Scenario: Include learners that were active in the relevant academic year
	Given SLD has informed the system of the following short courses
		| FirstName | LastName                | Uln   | StartDate  | ExpectedEndDate | WithdrawalDate | IsApproved |
		| Bob       | FullAY                  | 54321 | 2024-08-01 | 2025-07-31      |                | True       |
		| Tracey    | StartInAY               | 54322 | 2024-12-01 | 2025-10-01      |                | True       |
		| Seth      | FinishInAY              | 54323 | 2024-01-01 | 2024-12-01      |                | True       |
		| Samantha  | PreviousAY              | 54325 | 2023-09-01 | 2024-06-30      |                | True       |
		| Rudolph   | Unapproved              | 54326 | 2024-08-01 | 2025-07-31      |                | False      |
		| Lionel    | NextAY                  | 54324 | 2025-09-01 | 2026-06-30      |                | True       |
		| Grace     | CompletedInAY           | 54334 | 2024-09-01 | 2025-03-01      | 2025-03-01     | True       |
		| Alice     | WithdrawnPrevAY         | 54330 | 2023-09-01 | 2025-03-01      | 2024-06-01     | True       |
		| Dave      | WithdrawnPrevAY_PrevEnd | 54331 | 2023-09-01 | 2024-06-30      | 2024-06-01     | True       |
		| Emma      | WithdrawnInAY_PrevEnd   | 54332 | 2023-09-01 | 2024-06-30      | 2025-01-01     | True       |
		| Frank     | WithdrawnInAY_CurrEnd   | 54333 | 2023-09-01 | 2025-03-01      | 2025-01-01     | True       |
	When SLD requests the list of short courses for academic year 2425
	Then short courses are returned for the following Ulns
		| Uln   |
		| 54321 |
		| 54322 |
		| 54323 |
		| 54325 |
		| 54334 |
		| 54332 |
		| 54333 |
