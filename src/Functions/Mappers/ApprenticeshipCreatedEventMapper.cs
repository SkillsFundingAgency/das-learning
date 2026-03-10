using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Enums;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Learning.Functions.Mappers;

public static class ApprenticeshipCreatedEventMapper
{
    public static AddLearningCommand ToAddLearningCommand(ApprenticeshipCreatedEvent e)
    {
        return new AddLearningCommand
        {
            TrainingCode = e.TrainingCode,
            ActualStartDate = e.ActualStartDate,
            TotalPrice = e.PriceEpisodes[0].Cost,
            TrainingPrice = e.PriceEpisodes[0].TrainingPrice,
            EndPointAssessmentPrice = e.PriceEpisodes[0].EndPointAssessmentPrice,
            ApprovalsApprenticeshipId = e.ApprenticeshipId,
            EmployerAccountId = e.AccountId,
            FundingEmployerAccountId = e.TransferSenderId,
            FundingType = GetFundingType(e),
            LegalEntityName = e.LegalEntityName,
            PlannedEndDate = e.EndDate,
            UKPRN = e.ProviderId,
            Uln = e.Uln,
            DateOfBirth = e.DateOfBirth,
            FirstName = e.FirstName,
            LastName = e.LastName,
            ApprenticeshipHashedId = e.ApprenticeshipHashedId,
            FundingPlatform = e.IsOnFlexiPaymentPilot.HasValue
                ? (e.IsOnFlexiPaymentPilot.Value ? FundingPlatform.DAS : FundingPlatform.SLD)
                : null,
            AccountLegalEntityId = e.AccountLegalEntityId,
            TrainingCourseVersion = e.TrainingCourseVersion,
            PlannedStartDate = e.StartDate
        };
    }

    private static FundingType GetFundingType(ApprenticeshipCreatedEvent e)
    {
        if (e.TransferSenderId.HasValue)
            return FundingType.Transfer;

        if (e.ApprenticeshipEmployerTypeOnApproval == ApprenticeshipEmployerType.NonLevy)
            return FundingType.NonLevy;

        return FundingType.Levy;
    }
}