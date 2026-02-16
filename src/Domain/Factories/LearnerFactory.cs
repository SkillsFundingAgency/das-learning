using SFA.DAS.Learning.Domain.Apprenticeship;


namespace SFA.DAS.Learning.Domain.Factories;

public interface ILearnerFactory
{
    LearnerDomainModel CreateNew(string uln, DateTime dateOfBirth, string firstName, string lastName);
    LearnerDomainModel GetExisting(DataAccess.Entities.Learning.Learner model);
}

public class LearnerFactory : ILearnerFactory
{
    public LearnerDomainModel CreateNew(string uln, DateTime dateOfBirth, string firstName, string lastName)
    {
        return LearnerDomainModel.New(uln, dateOfBirth, firstName, lastName);
    }

    public LearnerDomainModel GetExisting(Learning.DataAccess.Entities.Learning.Learner entity)
    {
        return LearnerDomainModel.Get(entity);
    }
}