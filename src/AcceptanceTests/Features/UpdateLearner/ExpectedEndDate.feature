Feature: ExpectedEndDate

Tests for ExpectedEndDate under UpdateLearner

Scenario: ExpectedEndDate is moved earlier
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value        |
		| ExpectedEndDate | nextAY-05-31 |
	When the update request is sent
	Then the following Price details are stored
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-05-31 | 6000          | 500      |
	And the following changes are returned
		| Change          |
		| ExpectedEndDate |
	And the EpisodePrices are returned
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-05-31 | 6000          | 500      |
