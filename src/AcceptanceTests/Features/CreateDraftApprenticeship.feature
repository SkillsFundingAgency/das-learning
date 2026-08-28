Feature: CreateDraftApprenticeship

Validates behaviour of create draft apprenticeship. This end point also handles reinstating removed apprenticeships

Scenario: Apprenticeship does not exist, creates and returns 200
	Given No apprenticeship exists
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 200
	And the draft apprenticeship should be created
	And the CreateDraftApprenticeship response should include a NewApprenticeshipLearner change

# Reinstatement functionality todo in later ticket
# Scenario: Apprenticeship exists and is active, returns 204
# 	Given An apprenticeship has been created as part of the approvals journey
# 	When CreateDraftApprenticeship is called with apprenticeship details
# 	Then the CreateDraftApprenticeship endpoint should return a 204

Scenario: Apprenticeship exists but is removed, returns 200 and reinstates apprenticeship
	Given An apprenticeship has been created as part of the approvals journey
	And SLD inform us that a learner is to be removed
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 200
	And the apprenticeship should be reinstated
	And the CreateDraftApprenticeship response should not include a NewApprenticeshipLearner change

Scenario: Learner exists with a Short Course but no Apprenticeship, creates apprenticeship and flags a new apprenticeship learner
	Given SLD call the create short course endpoint with the following information
		| Uln      |
		| 76543210 |
	And No apprenticeship exists for learner with Uln 76543210
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 200
	And the draft apprenticeship should be created
	And the CreateDraftApprenticeship response should include a NewApprenticeshipLearner change
