Feature: CheckApprovedApprenticeshipExists

Verifies that existence checks work correctly.

Scenario: An approved apprenticeship exists for the given ULN, training code and start date
	Given a provider has an approved apprenticeship
	When the CheckApprovedApprenticeshipExists endpoint is called for that apprenticeship
	Then the response status code is 200

Scenario: No approved apprenticeship exists for the given ULN, training code and start date
	When the CheckApprovedApprenticeshipExists endpoint is called for an apprenticeship that was never created
	Then the response status code is 404
