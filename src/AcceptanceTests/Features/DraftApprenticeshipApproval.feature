Feature: Draft Apprenticeship Approval

Validates that when an ApprenticeshipCreatedEvent arrives for an apprenticeship that already exists in Learning
as an unapproved draft (created via SLD's draft POST, FLP-1537), the existing draft is approved rather than a
second apprenticeship record being created (FLP-2009).

Scenario: Draft apprenticeship is approved
	Given No apprenticeship exists
	When CreateDraftApprenticeship is called with apprenticeship details
	And the draft apprenticeship is approved by the approvals journey
	Then the existing apprenticeship record is approved and not duplicated
	And a LearningApprovedEvent event is published
