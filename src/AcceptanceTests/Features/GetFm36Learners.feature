Feature: GetFm36Learners

Tests that learnings are correctly looked up by key from GetLearningsForFm36 [HttpPost("{ukprn}/learnings/by-keys")].
Which learners are returned for FM36 (opt-in, academic year date rules and paging) is decided by the Earnings inner api, this endpoint only looks up the keys it is given.


Scenario: Only the requested learnings are returned
	Given The learner starts on currentAY-09-25 and has a plannedEndDate of currentAY-07-31
	When the GetLearningsForFm36 endpoint is called for the learner
	Then the fm36 learner should be Included

Scenario: Removed learners are excluded
	Given The learner starts on currentAY-09-25 and has a plannedEndDate of currentAY-07-31
	And SLD have previously informed us that the learner is to be removed
	When the GetLearningsForFm36 endpoint is called for the learner
	Then the fm36 learner should be Excluded
