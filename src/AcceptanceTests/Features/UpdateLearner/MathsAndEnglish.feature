﻿Feature: MathsAndEnglish

Tests for MathsAndEnglish under UpdateLearner

Scenario: Maths and English details are added
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                          |
		| MathsAndEnglish | course:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | StartDate       | PlannedEndDate | Amount |
		| maths  | currentAY-09-25 | nextAY-07-31   | 1000   |
	And the following changes are returned
		| Change          |
		| MathsAndEnglish |

Scenario: Maths and English details are added then removed
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                          |
		| MathsAndEnglish | course:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value |
		| MathsAndEnglish |       |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | StartDate       | PlannedEndDate | Amount |
	And the following changes are returned
		| Change          |
		| MathsAndEnglish |
