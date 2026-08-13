using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Models.UpdateModels;

#pragma warning disable CS8618 // Required properties must be set in the constructor
public class ApproveLearningContext
{
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public EmployerType EmployerType { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public long? TransferSenderId { get; set; }
    public string LegalEntityName { get; set; } = string.Empty;
}
#pragma warning restore CS8618
