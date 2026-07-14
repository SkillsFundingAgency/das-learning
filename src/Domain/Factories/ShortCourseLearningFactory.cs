using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface IShortCourseLearningFactory
{
    ShortCourseLearningDomainModel CreateNew(Guid learnerKey, string trainingCode);
    ShortCourseLearningDomainModel CreateNew(Guid learnerKey, string trainingCode, decimal price, SFA.DAS.Learning.Enums.LearningType learningType);

    ShortCourseLearningDomainModel GetExisting(DataAccess.Entities.Learning.ShortCourseLearning model);
}

public class ShortCourseLearningFactory : IShortCourseLearningFactory
{
    //todo sunset this
    public ShortCourseLearningDomainModel CreateNew(Guid learnerKey, string trainingCode)
    {
        return ShortCourseLearningDomainModel.New(learnerKey, trainingCode, 0, SFA.DAS.Learning.Enums.LearningType.Apprenticeship);
    }

    public ShortCourseLearningDomainModel CreateNew(Guid learnerKey, string trainingCode, decimal price, SFA.DAS.Learning.Enums.LearningType learningType)
    {
        return ShortCourseLearningDomainModel.New(learnerKey, trainingCode, price, learningType);
    }

    public ShortCourseLearningDomainModel GetExisting(ShortCourseLearning model)
    {
        return ShortCourseLearningDomainModel.Get(model);
    }
}