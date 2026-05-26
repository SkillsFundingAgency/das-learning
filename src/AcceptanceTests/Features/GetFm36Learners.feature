Feature: GetFm36Learners

Tests that learners are correctly returned from GetLearningsForFm36 [HttpGet("{ukprn}/{collectionYear}/{collectionPeriod}")]


Scenario: Correct learners returned when actual end date is derived from Withdrawn Date
	Given The learner starts on <StartDate> and has a plannedEndDate of <EndDate>
	And a withdrawn date of <WithdrawnDate> is set
	And the update request is sent
	When the GetLearningsForFm36 endpoint is called for currentAY-10-15
	Then the fm36 learner should be <Result>
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
	When the GetLearningsForFm36 endpoint is called for currentAY-10-15
	Then the fm36 learner should be <Result>
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