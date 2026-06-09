using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Domain.Events;

public class PersonalDetailsChangedEvent : IDomainEvent
{
    public static PersonalDetailsChangedEvent From(LearnerDomainModel learner, ShortCourseLearningDomainModel learning, ShortCourseEpisodeDomainModel shortCourseEpisode)
    {
        return new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = shortCourseEpisode.ApprovalsApprenticeshipId,
            LearningKey = learning.Key,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            EmailAddress = learner.EmailAddress
        };
    }

    public static PersonalDetailsChangedEvent From(LearnerDomainModel learner, ApprenticeshipLearningDomainModel learning, ApprenticeshipEpisodeDomainModel apprenticeshipEpisode)
    {
        return new PersonalDetailsChangedEvent
        {
            ApprovalsApprenticeshipId = apprenticeshipEpisode.ApprovalsApprenticeshipId,
            LearningKey = learning.Key,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            EmailAddress = learner.EmailAddress
        };
    }

    public Guid LearningKey { get; set; }
    public long ApprovalsApprenticeshipId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? EmailAddress { get; set; } = null!;
}