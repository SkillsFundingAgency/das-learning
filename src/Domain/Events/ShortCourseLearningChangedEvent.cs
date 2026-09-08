using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Events;

#pragma warning disable CS8618 // Required properties must be set in the constructor

public class ShortCourseLearningChangedEvent : IDomainEvent
{
    public static ShortCourseLearningChangedEvent From(
        ShortCourseLearningDomainModel learning,
        int? academicYear,
        ShortCourseLearningOperation operation,
        IReadOnlyCollection<ShortCourseUpdateChanges>? changes = null)
    {
        return new ShortCourseLearningChangedEvent
        {
            LearningKey = learning.Key,
            AcademicYear = academicYear,
            Operation = operation,
            Changes = changes?.ToList() ?? [],
            Snapshot = ShortCourseLearningSnapshot.From(learning)
        };
    }

    public Guid LearningKey { get; set; }
    public int? AcademicYear { get; set; }
    public ShortCourseLearningOperation Operation { get; set; }
    public List<ShortCourseUpdateChanges> Changes { get; set; }
    public ShortCourseLearningSnapshot Snapshot { get; set; }
}

public class ShortCourseLearningSnapshot
{
    public static ShortCourseLearningSnapshot From(ShortCourseLearningDomainModel learning)
    {
        return new ShortCourseLearningSnapshot
        {
            LearningKey = learning.Key,
            LearnerKey = learning.LearnerKey,
            TrainingCode = learning.TrainingCode,
            Price = learning.Price,
            LearningType = learning.LearningType,

            Episodes = learning.Episodes.Select(e => new ShortCourseEpisodeSnapshot
            {
                Key = e.Key,
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                LearnerRef = e.LearnerRef,
                IsApproved = e.IsApproved,
                IsRemoved = e.IsRemoved,
                StartDate = e.StartDate,
                ExpectedEndDate = e.ExpectedEndDate,
                WithdrawalDate = e.WithdrawalDate,
                WithdrawalReason = e.WithdrawalReason,
                CompletionDate = e.CompletionDate,
                ApprovalsApprenticeshipId = e.ApprovalsApprenticeshipId,
                EmployerType = e.EmployerType,
                TransferSenderId = e.TransferSenderId,
                ForceEarningsSync = e.ForceEarningsSync,

                Milestones = e.Milestones.Select(m => m.Milestone).ToList(),

                LearningSupport = e.LearningSupport.Select(ls => new ShortCourseLearningSupportSnapshot
                {
                    Key = ls.Key,
                    StartDate = ls.StartDate,
                    EndDate = ls.EndDate
                }).ToList()
            }).ToList()
        };
    }

    public Guid LearningKey { get; set; }
    public Guid LearnerKey { get; set; }
    public string TrainingCode { get; set; }
    public decimal Price { get; set; }
    public LearningType LearningType { get; set; }
    public List<ShortCourseEpisodeSnapshot> Episodes { get; set; }
}

public class ShortCourseEpisodeSnapshot
{
    public Guid Key { get; set; }
    public long Ukprn { get; set; }
    public long EmployerAccountId { get; set; }
    public string LearnerRef { get; set; }
    public bool IsApproved { get; set; }
    public bool IsRemoved { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime ExpectedEndDate { get; set; }
    public DateTime? WithdrawalDate { get; set; }
    public short? WithdrawalReason { get; set; }
    public DateTime? CompletionDate { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public EmployerType EmployerType { get; set; }
    public long? TransferSenderId { get; set; }
    public bool ForceEarningsSync { get; set; }
    public List<Milestone> Milestones { get; set; }
    public List<ShortCourseLearningSupportSnapshot> LearningSupport { get; set; }
}

public class ShortCourseLearningSupportSnapshot
{
    public Guid Key { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

#pragma warning restore CS8618 // Required properties must be set in the constructor
