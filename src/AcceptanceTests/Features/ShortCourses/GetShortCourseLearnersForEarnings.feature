Feature: GetShortCourseLearnersForEarnings

Scenario: Include learners active in the collection year with their learner and episode details
	Given SLD has informed the system of the following short courses
		| FirstName | LastName   | Uln   | StartDate  | ExpectedEndDate | IsApproved |
		| Bob       | FullAY     | 54321 | 2024-08-01 | 2025-07-31      | True       |
		| Tracey    | StartInAY  | 54322 | 2024-12-01 | 2025-10-01      | True       |
		| Seth      | FinishInAY | 54323 | 2024-01-01 | 2024-12-01      | False      |
		| Lionel    | NextAY     | 54324 | 2025-09-01 | 2026-06-30      | True       |
		| Samantha  | PreviousAY | 54325 | 2023-09-01 | 2024-06-30      | True       |
	When SLD requests short courses for earnings for collection year 2425
	Then short courses for earnings are returned with the following details
		| Uln   | FirstName | LastName   | CourseCode | IsApproved |
		| 54321 | Bob       | FullAY     | SC-ART1    | True       |
		| 54322 | Tracey    | StartInAY  | SC-ART1    | True       |
		| 54323 | Seth      | FinishInAY | SC-ART1    | False      |
