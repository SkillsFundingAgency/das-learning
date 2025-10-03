Feature: UpdateLearner

Tests for WithdrawalDate under UpdateLearner

Scenario: Apprenticeship withdrawal during learning recorded in ILR
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property       | Value           |
		| WithdrawalDate | currentAY-11-25 |
	When the update request is sent
	Then the Learning Status for the Learning is set to “Withdrawn”
	And the “last day of learning” for the Learning is set to currentAY-11-25
	And a LearningWithdrawnEvent is sent
	And the following changes are returned
		| Change     |
		| Withdrawal |

Scenario: Identical Withdrawal requests do not result in Earnings recalculation
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property       | Value           |
		| WithdrawalDate | currentAY-11-25 |
	When the update request is sent
	And an update request is sent again with the same data
	Then a LearningWithdrawnEvent is not sent
	And the following changes are returned
		| Change     |

Scenario: Apprentice Withdrawn via the ILR - following previous removal
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And SLD have previously informed us that the learner is to be removed
	And an update request has the following data
		| Property       | Value           |
		| WithdrawalDate | currentAY-11-25 |
	When the update request is sent
	Then the Learning Status for the Learning is set to “Withdrawn”
	And the “last day of learning” for the Learning is set to currentAY-11-25
	And a LearningWithdrawnEvent is sent
	And the following changes are returned
		| Change     |
		| Withdrawal |