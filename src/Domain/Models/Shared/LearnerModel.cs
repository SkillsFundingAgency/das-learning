using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Models.Shared;

#pragma warning disable CS8618
public class LearnerModel
{
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime DateOfBirth { get; set; }
}

public static class LearnerModelExtensions
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