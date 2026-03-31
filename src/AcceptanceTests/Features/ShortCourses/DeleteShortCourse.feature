Feature: DeleteShortCourse

Scenario: Delete short course sets WithdrawalDate to StartDate
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	When SLD calls the delete short course endpoint
	Then the short course episode WithdrawalDate equals the StartDate
