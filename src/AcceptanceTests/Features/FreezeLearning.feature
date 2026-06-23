Feature: FreezeLearning

A short summary of the feature

Scenario: Employer freezes short course
	Given SLD has informed the system that a new short course has been created
	And short course is approved via a ApprenticeshipCreatedEvent
	When Approvals inform us that the employer has <freezeType> a short course
	Then the short course learning is marked as 'payments frozen'
	Examples:
	| freezeType |
	| Paused      |
	| Stopped       |


Scenario: Frozen earnings are not paid
	Given SLD has informed the system that a new short course has been created
	And short course is approved via a ApprenticeshipCreatedEvent
	When Approvals inform us that the employer has Paused a short course
	Then a paymentStatus updated event is published with a frozen flag set to true