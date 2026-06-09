Feature: CreateDraftApprenticeship

Validates behaviour of create draft apprenticeship. This end point also handles reinstating removed apprenticeships

# Note this test should be replaced when the create functionality is introduced
# at time of writing only reinstating removed apprenticeships is supported
Scenario: Apprenticeship does not exist, returns 204 
	Given No apprenticeship exists
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 204

Scenario: Apprenticeship exists and is active, returns 204 
	Given An apprenticeship has been created as part of the approvals journey
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 204

Scenario: Apprenticeship exists but is removed, returns 200 and reinstates apprenticeship
	Given An apprenticeship has been created as part of the approvals journey
	And SLD inform us that a learner is to be removed
	When CreateDraftApprenticeship is called with apprenticeship details
	Then the CreateDraftApprenticeship endpoint should return a 200
	And the apprenticeship should be reinstated
