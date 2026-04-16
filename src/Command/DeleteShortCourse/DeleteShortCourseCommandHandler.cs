using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using System.Linq;

namespace SFA.DAS.Learning.Command.DeleteShortCourse;

public class DeleteShortCourseCommandHandler(
    ILogger<DeleteShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository)
    : ICommandHandler<DeleteShortCourseCommand, DeleteShortCourseResult>
{
    public async Task<DeleteShortCourseResult> Handle(DeleteShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling DeleteShortCourseCommand for LearningKey {LearningKey}", command.LearningKey);

        var learning = await repository.Get(command.LearningKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning with key {command.LearningKey} not found.");

        var deleted = learning.Delete(command.Ukprn);

        if (!deleted)
        {
            logger.LogInformation("Short course {LearningKey} is not approved; delete request ignored.", command.LearningKey);
            return new DeleteShortCourseResult { WasDeleted = false };
        }

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        return new DeleteShortCourseResult
        {
            WasDeleted = true,
            LearningKey = learning.Key,
            LearnerKey = learning.LearnerKey,
            CompletionDate = learning.CompletionDate,
            Learner = new ShortCourseLearningResultLearner
            {
                Uln = learner!.Uln,
                FirstName = learner.FirstName,
                LastName = learner.LastName,
                DateOfBirth = learner.DateOfBirth
            },
            Episodes = learning.Episodes.Where(e => e.Ukprn == command.Ukprn).Select(e => new ShortCourseLearningResultEpisode
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
            }).ToArray()
        };
    }
}
