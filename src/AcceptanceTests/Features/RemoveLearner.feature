Feature: RemoveLearner

A short summary of the feature

Scenario: Apprentice removed from the ILR
	Given that an apprentice record has been approved by an employer previously
	When SLD inform us that a learner is to be removed
	Then the Learning Status for the Learning is set to Withdrawn
	And “last day of learning” for the Learning is set to its “Learning Start Date”
	And a LearningWithdrawnEvent is sent
