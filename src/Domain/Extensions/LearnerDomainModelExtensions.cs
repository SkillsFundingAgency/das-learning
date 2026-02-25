using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;

namespace SFA.DAS.Learning.Domain.Extensions;

public static class LearnerDomainModelExtensions
{
    public static LearnerModel ToModel(this LearnerDomainModel domainModel)
    {
        return new LearnerModel
        {
            Uln = domainModel.Uln,
            FirstName = domainModel.FirstName,
            LastName = domainModel.LastName,
            EmailAddress = domainModel.EmailAddress,
            DateOfBirth = domainModel.DateOfBirth
        };
    }

}
