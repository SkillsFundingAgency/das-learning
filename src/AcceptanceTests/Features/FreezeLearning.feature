Feature: FreezeLearning

A short summary of the feature

Scenario: Employer freezes short course
	Given a learner on an approved short course
	When Approvals inform us that the employer has <freezeType> 
	Then the short course learning is marked as 'payments frozen'
	Examples:

	| freezeType |
	| Pause      |
	| Stop       |


Scenario: When earnings are frozen, recalculating doesn’t unfreeze them
	Given a learner on an approved short course
	And earnings for that learner have been frozen by the employer
	When earnings are recalculated
	Then the 'payments frozen' status of the learning is not changed


Scenario: Frozen earnings are not paid
	Given there are frozen earnings on a short course
	When the earnings for the short course are sent to PV2 for payment
	Then the short course is flagged as 'payments frozen'