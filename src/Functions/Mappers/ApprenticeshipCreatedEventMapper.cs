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
            TransferSenderId = e.TransferSenderId,
            LegalEntityName = e.LegalEntityName,
            PlannedEndDate = e.EndDate,
            UKPRN = e.ProviderId,
            Uln = e.Uln,
            DateOfBirth = e.DateOfBirth,
            FirstName = e.FirstName,
            LastName = e.LastName,
            ApprenticeshipHashedId = e.ApprenticeshipHashedId,
            AccountLegalEntityId = e.AccountLegalEntityId,
            TrainingCourseVersion = e.TrainingCourseVersion,
            PlannedStartDate = e.StartDate,
            LearningType = (Enums.LearningType) e.LearningType,
            EmployerType = GetEmployerType(e)
        };
    }

    private static EmployerType GetEmployerType(ApprenticeshipCreatedEvent e)
    {
        if (e.ApprenticeshipEmployerTypeOnApproval == ApprenticeshipEmployerType.Levy)
            return EmployerType.Levy;

        return EmployerType.NonLevy;
    }
}