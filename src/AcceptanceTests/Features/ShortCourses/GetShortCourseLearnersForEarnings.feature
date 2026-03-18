Feature: GetShortCourseLearnersForEarnings

Scenario: Include learners active in the collection year with their learner and episode details
	Given SLD has informed the system of the following short courses
		| FirstName | LastName           | Uln   | StartDate  | ExpectedEndDate | IsApproved | Price |
		| Bob       | FullAY             | 54321 | 2024-08-01 | 2025-07-31      | True       |  1500 |
		| Tracey    | StartInAY          | 54322 | 2024-12-01 | 2025-10-01      | True       |  2000 |
		| Seth      | FinishInAY         | 54323 | 2024-01-01 | 2024-12-01      | False      |   500 |
		| Samantha  | OverdueStillActive | 54325 | 2023-09-01 | 2024-06-30      | True       |   750 |
		| Lionel    | NextAY             | 54324 | 2025-09-01 | 2026-06-30      | True       |  1000 |
	When SLD requests short courses for earnings for collection year 2425
	Then short courses for earnings are returned with the following details
		| Uln   | FirstName | LastName           | CourseCode | IsApproved | Price |
		| 54321 | Bob       | FullAY             | SC-ART1    | True       |  1500 |
		| 54322 | Tracey    | StartInAY          | SC-ART1    | True       |  2000 |
		| 54323 | Seth      | FinishInAY         | SC-ART1    | False      |   500 |
		| 54325 | Samantha  | OverdueStillActive | SC-ART1    | True       |   750 |
