using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IShortCourseLearningFactory
{
    ShortCourseLearningDomainModel CreateNew(
        DateTime? withdrawalDate,
        DateTime expectedEndDate,
        DateTime? completionDate,
        bool isApproved);

    ShortCourseLearningDomainModel GetExisting(DataAccess.Entities.Learning.ShortCourseLearning model);
}