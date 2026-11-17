Feature: FundingPlatform

Changes are only sent to Approvals for learners paid by DAS (FLP-2141). Learning is still updated for every learner.
The test config opts provider 10000001 into payments from the start of the current academic year.

Scenario: A new learner at the opted-in provider is paid by DAS and a start date change needs further approval
	Given No apprenticeship exists for a learner with the payments opted-in provider
	And CreateDraftApprenticeship is called with apprenticeship details
	And the draft apprenticeship has been approved
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-11-25 trainingPrice:6000 epaoPrice:500 |
	When the update request is sent
	Then the apprenticeship episode FundingPlatform is DAS
	And the update result says further approval is needed
	And further approval needed is set on the apprenticeship episode

Scenario: A new learner starting before the opt-in date is not paid by DAS and a start date change does not need further approval
	Given No apprenticeship exists for a learner with the payments opted-in provider
	And an update request has the following data
		| Property | Value                                                      |
		| Prices   | fromDate:previousAY-07-25 trainingPrice:6000 epaoPrice:500 |
	And CreateDraftApprenticeship is called with apprenticeship details
	And the draft apprenticeship has been approved
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-11-25 trainingPrice:6000 epaoPrice:500 |
	When the update request is sent
	Then the apprenticeship episode FundingPlatform is SLD
	And the update result says further approval is not needed
	And further approval needed is not set on the apprenticeship episode

Scenario: A start date change for a learner not paid by DAS updates Learning without needing further approval
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And the apprenticeship episode FundingPlatform is set to SLD
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-11-25 trainingPrice:6000 epaoPrice:500 |
	When the update request is sent
	Then the following changes are returned
		| Change    |
		| StartDate |
		| Prices    |
	And the update result says further approval is not needed
	And further approval needed is not set on the apprenticeship episode

Scenario: A price change for a learner not paid by DAS updates Learning without needing further approval
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And the apprenticeship episode FundingPlatform is set to SLD
	And an update request has the following data
		| Property | Value                                                     |
		| Prices   | fromDate:currentAY-09-25 trainingPrice:5500 epaoPrice:500 |
	When the update request is sent
	Then the following changes are returned
		| Change |
		| Prices |
	And the update result says further approval is not needed
	And further approval needed is not set on the apprenticeship episode

Scenario: A withdrawal for a learner not paid by DAS updates Learning without informing Approvals
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And the apprenticeship episode FundingPlatform is set to SLD
	And an update request has the following data
		| Property             | Value           |
		| WithdrawalDate       | currentAY-11-25 |
		| WithdrawalReasonCode | 43              |
	When the update request is sent
	Then the “last day of learning” for the Learning is set to currentAY-11-25
	And the following changes are returned
		| Change     |
		| Withdrawal |
	And an LearningWithdrawnEvent is not sent

Scenario: A withdrawal reversal for a learner not paid by DAS updates Learning without informing Approvals
	Given There is an apprenticeship with the following details
		| StartDate       | EndDate      | TrainingPrice | EpaPrice |
		| currentAY-09-25 | nextAY-07-31 | 6000          | 500      |
	And the apprenticeship episode FundingPlatform is set to SLD
	And an update request has the following data
		| Property             | Value           |
		| WithdrawalDate       | currentAY-11-25 |
		| WithdrawalReasonCode | 43              |
	And the update request is sent
	And an update request has the following data
		| Property             | Value |
		| WithdrawalDate       | null  |
		| WithdrawalReasonCode | null  |
	When the update request is sent
	Then the “last day of learning” for the Learning is set to null
	And the following changes are returned
		| Change            |
		| ReverseWithdrawal |
	And an LearningWithdrawalRevertedEvent is not sent
