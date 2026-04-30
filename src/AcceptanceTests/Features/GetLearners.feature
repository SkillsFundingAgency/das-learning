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

Scenario: Correct learners returned when actual end date is derived from Withdrawn Date
	Given The learner starts on <StartDate> and has a plannedEndDate of <EndDate>
	And a withdrawn date of <WithdrawnDate> is set
	And the update request is sent
	When the GetByAcademicYear endpoint is called for currentAY-10-15
	Then the learner should be <Result>
	Examples: 
	| StartDate        | EndDate          | WithdrawnDate    | Result   |
	| currentAY-09-25  | currentAY-07-31  | null             | Included |
	| currentAY-11-25  | currentAY-07-31  | null             | Included |
	| currentAY-09-25  | currentAY-07-31  | currentAY-06-30  | Included |
	| previousAY-09-25 | previousAY-07-31 | null             | Included |
	| previousAY-09-25 | previousAY-07-31 | previousAY-06-30 | Excluded |
	| previousAY-09-25 | currentAY-07-31  | previousAY-06-30 | Excluded |
	| previousAY-09-25 | previousAY-07-31 | currentAY-06-30  | Included |
	| previousAY-09-25 | currentAY-07-31  | currentAY-06-30  | Included |
	| previousAY-09-25 | currentAY-07-31  | null             | Included |
	| nextAY-09-25     | nextAY-07-31     | null             | Excluded |

Scenario: Correct learners returned when actual end date is derived from Completion Date
	Given The learner starts on <StartDate> and has a plannedEndDate of <EndDate>
	And a completion date of <CompletionDate> is set
	And the update request is sent
	When the GetByAcademicYear endpoint is called for currentAY-10-15
	Then the learner should be <Result>
	Examples: 
	| StartDate        | EndDate          | CompletionDate   | Result   |
	| currentAY-09-25  | currentAY-07-31  | null             | Included |
	| currentAY-11-25  | currentAY-07-31  | null             | Included |
	| currentAY-09-25  | currentAY-07-31  | currentAY-06-30  | Included |
	| previousAY-09-25 | previousAY-07-31 | null             | Included |
	| previousAY-09-25 | previousAY-07-31 | previousAY-06-30 | Excluded |
	| previousAY-09-25 | currentAY-07-31  | previousAY-06-30 | Excluded |
	| previousAY-09-25 | previousAY-07-31 | currentAY-06-30  | Included |
	| previousAY-09-25 | currentAY-07-31  | currentAY-06-30  | Included |
	| previousAY-09-25 | currentAY-07-31  | null             | Included |
	| nextAY-09-25     | nextAY-07-31     | null             | Excluded |