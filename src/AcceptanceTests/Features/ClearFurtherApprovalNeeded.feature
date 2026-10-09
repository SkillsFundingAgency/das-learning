Feature: ClearFurtherApprovalNeeded

Tests for flagging an approved apprenticeship episode as needing further approval when its start date changes, and clearing that flag.

Scenario: A start date change flags the episode and the flag can then be cleared
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-11-25 trainingPrice:5500 epaoPrice:400 |
	And the update request is sent
	Then the update result says further approval is needed
	And further approval needed is set on the apprenticeship episode
	When further approval needed is cleared for the apprenticeship episode
	Then the clear request returns 204
	And further approval needed is not set on the apprenticeship episode

Scenario: Clearing the flag twice is harmless
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	When further approval needed is cleared for the apprenticeship episode
	And further approval needed is cleared for the apprenticeship episode
	Then the clear request returns 204
	And further approval needed is not set on the apprenticeship episode

Scenario: Clearing the flag for a learning that does not exist
	When further approval needed is cleared for a learning that does not exist
	Then the clear request returns 404

Scenario: Clearing the flag for an episode that does not exist
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	When further approval needed is cleared for an episode that does not exist
	Then the clear request returns 404
