using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories
{
    public interface ILearningFactory
    {
        LearningDomainModel CreateNew(
            long approvalsApprenticeshipId,
            string uln, 
            DateTime dateOfBirth,
            string firstName, 
            string lastName, 
            string apprenticeshipHashedId);
        
        LearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
    }
}
