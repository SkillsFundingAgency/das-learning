Feature: EnglishAndMaths

Tests for EnglishAndMaths under UpdateLearner

Scenario: English and Maths details are added
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   |
	And the following changes are returned
		| Change          |
		| EnglishAndMaths |
	And the learning history is maintained

Scenario: English and Maths details are added then removed
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value |
		| EnglishAndMaths |       |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | StartDate       | PlannedEndDate | Amount |
	And the following changes are returned
		| Change          |
		| EnglishAndMaths |
	And the learning history is maintained

Scenario: English and Maths details are added then withdrawn back to the start
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                         |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 withdrawalDate:currentAY-09-25 |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | WithdrawalDate  |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-09-25 |
	And the following changes are returned
		| Change                    |
		| EnglishAndMathsWithdrawal |
	And the learning history is maintained

Scenario: English and Maths pause date is set
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                     |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 pauseDate:currentAY-12-25 |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | PauseDate       |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25 |
	And the following changes are returned
		| Change          |
		| EnglishAndMaths |
	And the learning history is maintained

Scenario: English and Maths pause date is moved later
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                                                     |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 pauseDate:currentAY-06-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                     |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 pauseDate:currentAY-12-25 |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | PauseDate       |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25 |
	And the following changes are returned
		| Change          |
		| EnglishAndMaths |
	And the learning history is maintained

Scenario: English and Maths pause date is removed
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                                                     |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 pauseDate:currentAY-06-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                           |
		| EnglishAndMaths | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	When the update request is sent
	Then the following English and Maths details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | PauseDate |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   |           |
	And the following changes are returned
		| Change          |
		| EnglishAndMaths |
	And the learning history is maintained