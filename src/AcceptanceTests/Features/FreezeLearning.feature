Feature: FreezeLearning

This tests the FreezeLearning feature, which is responsible for freezing learning when an employer freezes a short course.

Scenario: Employer freezes short course
	Given SLD has informed the system that a new short course has been created
	And short course is approved via a ApprenticeshipCreatedEvent
	When Approvals inform us that the employer has <freezeType> a short course
	Then the short course learning is marked as 'payments frozen'
	And a paymentStatus updated event is published with a frozen flag set to true
	Examples:
	| freezeType |
	| Paused     |
	| Stopped    |
