using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IShortCourseLearningFactory
{
    ShortCourseLearningDomainModel CreateNew(Guid learnerKey);

    ShortCourseLearningDomainModel GetExisting(DataAccess.Entities.Learning.ShortCourseLearning model);
}

public class ShortCourseLearningFactory : IShortCourseLearningFactory
{
    public ShortCourseLearningDomainModel CreateNew(Guid learnerKey)
    {
        return ShortCourseLearningDomainModel.New(learnerKey);
    }

    public ShortCourseLearningDomainModel GetExisting(ShortCourseLearning model)
    {
        return ShortCourseLearningDomainModel.Get(model);
    }
}