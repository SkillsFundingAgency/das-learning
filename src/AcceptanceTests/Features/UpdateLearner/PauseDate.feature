Feature: PauseDate

Tests for PauseDate under UpdateLearner

Scenario: Apprenticeship pause date is set
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property  | Value           |
		| PauseDate | currentAY-11-25 |
	When the update request is sent
	Then the Pause Date for the Learning is set to currentAY-11-25
	And the following changes are returned
		| Change                 |
		| BreakInLearningStarted |

Scenario: Apprenticeship pause date is un set
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property  | Value           |
		| PauseDate | currentAY-11-25 |
	And the update request is sent
	And an update request has the following data
		| Property  | Value |
		| PauseDate | null  |
	When the update request is sent
	Then the Pause Date for the Learning is set to null
	And the following changes are returned
		| Change                 |
		| BreakInLearningRemoved |
