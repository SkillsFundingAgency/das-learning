Feature: CompletionDate

Tests for CompletionDate under UpdateLearner

Scenario: Apprenticeship completion date is set
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property       | Value           |
		| CompletionDate | currentAY-11-25 |
	When the update request is sent
	Then the Completion Date for the Learning is set to currentAY-11-25
	And the following changes are returned
		| Change         |
		| CompletionDate |

Scenario: Apprenticeship completion date is un set
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property       | Value           |
		| CompletionDate | currentAY-11-25 |
	And the update request is sent
	And an update request has the following data
		| Property       | Value |
		| CompletionDate | null  |
	When the update request is sent
	Then the Completion Date for the Learning is set to null
	And the following changes are returned
		| Change         |
		| CompletionDate |