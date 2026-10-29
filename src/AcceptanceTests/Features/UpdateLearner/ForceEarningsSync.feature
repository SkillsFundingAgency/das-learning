Feature: ForceEarningsSync

Tests that a forced earnings sync is reported as a change under UpdateLearner

Scenario: Update includes ForceEarningsSync when set and excludes it when unset
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And the update request is sent
	And the apprenticeship episode ForceEarningsSync is set to True
	When the update request is sent
	Then the following changes are returned
		| Change            |
		| ForceEarningsSync |
	And the learning history is maintained
	Given the apprenticeship episode ForceEarningsSync is set to False
	When the update request is sent
	Then the following changes are returned
		| Change |
