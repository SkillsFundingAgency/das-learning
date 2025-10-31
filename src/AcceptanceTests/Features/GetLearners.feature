Feature: GetLearners

A short summary of the feature


Scenario: Include learners that were active in the relevant academic year
	Given a provider has learners
		| Category                   | Number |
		| CurrentlyActive            | 5      |
		| CompletedInPreviousYear    | 3      |
		| StartNextYear              | 2      |
		| WithdrawnInPreviousYear    | 4      |
		| WithdrawnToStartInThisYear | 1      |
		| WithdrawnInThisYear        | 2      |
	When SLD requests the list of active Learnings for the provider in an academic year
	Then the list of Learnings sent includes any Learnings where the Learning was active in that year but have now been withdrawn while in-learning
	And the list of Learnings sent includes any Learnings where the Learning is or was active in that year

Scenario: Do not include learners that were withdrawn back to the start
	Given a provider has learners
		| Category                   | Number |
		| CurrentlyActive            | 5      |
		| CompletedInPreviousYear    | 3      |
		| StartNextYear              | 2      |
		| WithdrawnInPreviousYear    | 4      |
		| WithdrawnToStartInThisYear | 1      |
		| WithdrawnInThisYear        | 2      |
	When SLD requests the list of active Learnings for the provider in an academic year
	Then the list of Learnings sent does not include any Learnings where the Learning was active in that year and has been been withdrawn back to the Learning's start date

Scenario: Do not include learners that were withdrawn prior to the academic year
	Given a provider has learners
		| Category                   | Number |
		| CurrentlyActive            | 5      |
		| CompletedInPreviousYear    | 3      |
		| StartNextYear              | 2      |
		| WithdrawnInPreviousYear    | 4      |
		| WithdrawnToStartInThisYear | 1      |
		| WithdrawnInThisYear        | 2      |
	When SLD requests the list of active Learnings for the provider in an academic year
	Then the list of Learnings sent does not include any Learnings where the Learning was withdrawn in a prior year