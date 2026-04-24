using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Models.Dtos;

namespace SFA.DAS.Learning.Command.Mappers;

public interface IShortCourseLearningDomainModelMapper
{
    T Map<T>(ShortCourseLearningDomainModel learning, LearnerDomainModel learner, long ukprn) where T : ShortCourseLearningDto, new();
}

public class ShortCourseLearningDomainModelMapper : IShortCourseLearningDomainModelMapper
{
    public T Map<T>(ShortCourseLearningDomainModel learning, LearnerDomainModel learner, long ukprn)
        where T : ShortCourseLearningDto, new()
    {
        return new T
        {
            LearningKey = learning.Key,
            LearnerKey = learning.LearnerKey,
            CompletionDate = learning.CompletionDate,
            Learner = new ShortCourseLearnerDto
            {
                Uln = learner.Uln,
                FirstName = learner.FirstName,
                LastName = learner.LastName,
                DateOfBirth = learner.DateOfBirth
            },
            Episodes = learning.Episodes.Where(e => e.Ukprn == ukprn).Select(e => new ShortCourseEpisodeDto
            {
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                CourseCode = e.TrainingCode,
                CourseType = CourseTypeConstants.ShortCourse,
                LearningType = e.LearningType,
                StartDate = e.StartDate,
                AgeAtStart = learner.AgeOnDate(e.StartDate),
                PlannedEndDate = e.ExpectedEndDate,
                WithdrawalDate = e.WithdrawalDate,
                IsApproved = e.IsApproved,
                Price = e.Price,
                LearnerRef = e.LearnerRef,
                EmployerType = e.EmployerType,
                ApprovalsApprenticeshipId = e.ApprovalsApprenticeshipId,
                TransferSenderId = e.TransferSenderId
            }).ToArray()
        };
    }
}
