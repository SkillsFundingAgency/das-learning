using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories
{
    public interface IApprenticeshipLearningFactory
    {
        ApprenticeshipLearningDomainModel CreateNew(
            long approvalsApprenticeshipId,
            string uln, 
            DateTime dateOfBirth,
            string firstName, 
            string lastName, 
            string apprenticeshipHashedId);
        
        ApprenticeshipLearningDomainModel GetExisting(DataAccess.Entities.Learning.ApprenticeshipLearning model);
    }
}
