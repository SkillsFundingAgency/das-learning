using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Factories
{
    public class ApprenticeshipLearningFactory : IApprenticeshipLearningFactory
    {
        public ApprenticeshipLearningDomainModel CreateNew(
            long approvalsApprenticeshipId,
            string uln, 
            DateTime dateOfBirth,
            string firstName, 
            string lastName, 
            string apprenticeshipHashedId)
        {
            return ApprenticeshipLearningDomainModel.New(
                approvalsApprenticeshipId,
                uln,
                dateOfBirth,
                firstName,
                lastName,
                apprenticeshipHashedId);
        }

        public ApprenticeshipLearningDomainModel GetExisting(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
        {
            return ApprenticeshipLearningDomainModel.Get(entity);
        }
    }
}
