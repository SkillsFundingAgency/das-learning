using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public class ShortCourseLearningFactory : IShortCourseLearningFactory
{
    public ShortCourseLearningDomainModel CreateNew(
        DateTime? withdrawalDate,
        DateTime expectedEndDate,
        DateTime? completionDate,
        bool isApproved)
    {
        return ShortCourseLearningDomainModel.New(
            Guid.NewGuid(), 
            withdrawalDate,
            expectedEndDate,
            completionDate,
            isApproved);
    }

    public ShortCourseLearningDomainModel GetExisting(ShortCourseLearning model)
    {
        return ShortCourseLearningDomainModel.Get(model);
    }
}