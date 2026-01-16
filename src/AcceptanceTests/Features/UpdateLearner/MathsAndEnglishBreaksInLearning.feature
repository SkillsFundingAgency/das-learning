Feature: MathsAndEnglishBreaksInLearning

A short summary of the feature


Scenario: When break in learning does not change, no change is recorded
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-25 breakInLearningEnd:nextAY-03-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                                                                |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-25 breakInLearningEnd:nextAY-03-25 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | BreakInLearningStart | BreakInLearningEnd | BreakInLearningPriorPeriodExpectedEndDate |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25      | nextAY-03-25       | nextAY-07-31                              |
	And the following changes are returned
		| Change                                 |

Scenario: A completed break in learning is recorded
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                           |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                                                                |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-25 breakInLearningEnd:nextAY-03-25 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | BreakInLearningStart | BreakInLearningEnd | BreakInLearningPriorPeriodExpectedEndDate |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25      | nextAY-03-25       | nextAY-07-31                              |
	And the following changes are returned
		| Change                                 |
		| EnglishAndMathsBreaksInLearningUpdated |
	And the learning history is maintained

Scenario: Return from a break in learning
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                                                     |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 pauseDate:currentAY-12-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                                                                                          |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-25 breakInLearningEnd:nextAY-03-25 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | BreakInLearningStart | BreakInLearningEnd | BreakInLearningPriorPeriodExpectedEndDate |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25      | nextAY-03-25       | nextAY-07-31                              |
	And the following changes are returned
		| Change                                 |
		| MathsAndEnglish                        | 
		| EnglishAndMathsBreaksInLearningUpdated |
		# there are 2 changes returned because the pauseDate is removed and that is MathsAndEnglish change
		# ultimately both trigger the same action in the outer
	And the learning history is maintained


Scenario: Training provider corrects a previously recorded return from a break in learning
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                                                                                                |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-20 breakInLearningEnd:nextAY-03-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                                                                                                |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-25 breakInLearningEnd:nextAY-03-25 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount | BreakInLearningStart | BreakInLearningEnd | BreakInLearningPriorPeriodExpectedEndDate |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   | currentAY-12-25      | nextAY-03-25       | nextAY-07-31                              |
	And the following changes are returned
		| Change                                 |
		| EnglishAndMathsBreaksInLearningUpdated |
	And the learning history is maintained

Scenario: Training provider removes a previously recorded return from a break in learning
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And an update request has the following data
		| Property        | Value                                                                                                                                                                |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 breakInLearningStart:currentAY-12-20 breakInLearningEnd:nextAY-03-25 |
	And the update request is sent
	And an update request has the following data
		| Property        | Value                                                                                           |
		| MathsAndEnglish | course:test learnAimRef:maths startDate:currentAY-09-25 plannedEndDate:nextAY-07-31 amount:1000 |
	When the update request is sent
	Then the following maths and english details are stored
		| Course | LearnAimRef | StartDate       | PlannedEndDate | Amount |
		| test   | maths       | currentAY-09-25 | nextAY-07-31   | 1000   |
	And the following changes are returned
		| Change                                 |
		| EnglishAndMathsBreaksInLearningUpdated |
	And the learning history is maintained


#  Testing scenario for consideration:
#
#     No changes, learner returns after 3 months
#     
#     Learner has a price increase when returning from BiL
#     
#     Original end date pushed back
#     
#     Withdraw learner on same day as return from Break in Learning
#     
#     Withdraw learner during learning after return from Break in Learning
#     
#     Complete learning after return from Break in Learning
#     
#     Remove learner after return from Break in Learning
#     
#     No Changes, learner returns after 3 months and goes on Bil again after 5 months
#     
#     "Original" end pushed pack after return from second BiL
#     
#     Return from Break in Learning is corrected
#     
#     Return from Break in Learning is removed
#     
#     Return from Break in Learning is recorded – LearnAimRef changed