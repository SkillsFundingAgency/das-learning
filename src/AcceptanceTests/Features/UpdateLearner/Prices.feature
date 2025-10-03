Feature: Prices

Tests for Prices under UpdateLearner

Scenario: Prices are updated
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-11-25 trainingPrice:5500 epaoPrice:400 |
	When the update request is sent
	Then the following Price details are stored
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-11-25 | nextAY-07-31 | 5500          | 400      |
	And the following changes are returned
		| Change          |
		| Prices |
	And the EpisodePrices are returned
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-11-25 | nextAY-07-31 | 5500          | 400      |
