Feature: RemoveLearner

A short summary of the feature

Scenario: Apprentice removed from the ILR
	Given that an apprentice record has been approved by an employer previously
	When SLD inform us that a learner is to be removed
	Then “last day of learning” for the Learning is set to its “Learning Start Date”
	And an LearningRemovedEvent is sent

Scenario: Apprentice Reinstatement
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And SLD have previously informed us that the learner is to be removed
	And an update request has the following data
		| Property  | Value           |
		| PauseDate | currentAY-05-01 |
	When the update request is sent
	Then a LearningReinstatedEvent is sent
	And the following changes are returned
		| Change        |
		| Reinstatement |
	And the learning history is maintained