Feature: RemoveShortCourse

Scenario: Remove short course sets IsRemoved to True
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	When SLD remove the short course
	Then the short course episode IsRemoved is set to True

Scenario: Remove approved short course publishes LearningRemovedEvent
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	When SLD remove the short course
	Then an LearningRemovedEvent is sent

Scenario: Reinstate removed short course sets IsRemoved to False and publishes LearningReinstatedEvent
	Given SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	And SLD remove the short course
	When SLD call the create short course endpoint with the following information
		| StartDate  | IsApproved |
		| 2024-08-01 | True       |
	Then the short course episode IsRemoved is set to False
	And an LearningReinstatedEvent is sent
