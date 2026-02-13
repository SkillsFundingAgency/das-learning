using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IShortCourseLearningFactory
{
    ShortCourseLearningDomainModel CreateNew(
        Guid learnerKey,
        DateTime? completionDate);

    ShortCourseLearningDomainModel GetExisting(DataAccess.Entities.Learning.ShortCourseLearning model);
}