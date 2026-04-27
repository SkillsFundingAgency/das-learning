Feature: DeleteShortCourse

Scenario: Delete short course sets WithdrawalDate to StartDate
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	When SLD delete the short course
	Then the short course episode WithdrawalDate equals the StartDate

Scenario: Delete approved short course publishes LearningRemovedEvent
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	When SLD delete the short course
	Then an LearningRemovedEvent is sent
