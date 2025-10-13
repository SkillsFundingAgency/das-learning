Feature: LearningSupport

Tests for LearningSupport under UpdateLearner

Scenario: LearningSupport details are added
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                          |
		| LearningSupport | startDate:currentAY-09-25 endDate:nextAY-07-31 |
	When the update request is sent
	Then the following LearningSupport details are stored
		| StartDate       | EndDate      |
		| currentAY-09-25 | nextAY-07-31 |
	And the following changes are returned
		| Change          |
		| LearningSupport |

Scenario: LearningSupport details are added then removed
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                          |
		| LearningSupport | startDate:currentAY-09-25 endDate:nextAY-07-31 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value |
		| LearningSupport |       |
	When the update request is sent
	Then the following LearningSupport details are stored
		| StartDate       | EndDate      |
	And the following changes are returned
		| Change          |
		| LearningSupport |