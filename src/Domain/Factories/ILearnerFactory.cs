using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories;

public interface ILearnerFactory
{
    LearnerDomainModel CreateNew(string uln, DateTime dateOfBirth, string firstName, string lastName);
    LearnerDomainModel GetExisting(DataAccess.Entities.Learning.Learner model);
}
