using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.AddLearning
{
    public class AddLearningCommand : ICommand
    {
        public string Uln { get; set; }
        public string TrainingCode { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public long UKPRN { get; set; }
        public long EmployerAccountId { get; set; }
        public string LegalEntityName { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? TrainingPrice { get; set; } 
        public decimal? EndPointAssessmentPrice { get; set; }
        public long? FundingEmployerAccountId { get; set; }
        public FundingType FundingType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public FundingPlatform? FundingPlatform { get; set; }
        public string ApprenticeshipHashedId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string TrainingCourseVersion { get; set; }
    }
}
