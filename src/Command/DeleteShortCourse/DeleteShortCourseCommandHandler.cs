using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using ShortCourseLearnerDto = SFA.DAS.Learning.Models.Dtos.ShortCourseLearner;
using ShortCourseEpisodeDto = SFA.DAS.Learning.Models.Dtos.ShortCourseEpisode;

namespace SFA.DAS.Learning.Command.DeleteShortCourse;

public class DeleteShortCourseCommandHandler(
    ILogger<DeleteShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository)
    : ICommandHandler<DeleteShortCourseCommand, DeleteShortCourseResult?>
{
    public async Task<DeleteShortCourseResult?> Handle(DeleteShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling DeleteShortCourseCommand for LearningKey {LearningKey}", command.LearningKey);

        var learning = await repository.Get(command.LearningKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning with key {command.LearningKey} not found.");

        var episode = learning.Episodes.SingleOrDefault(e => e.Ukprn == command.Ukprn);

        if (episode == null || !episode.IsApproved)
        {
            logger.LogInformation("Short course {LearningKey} is not approved; delete request ignored.", command.LearningKey);
            return null;
        }

        episode.Delete();

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        return new DeleteShortCourseResult
        {
            LearningKey = learning.Key,
            CompletionDate = learning.CompletionDate,
            Learner = new ShortCourseLearnerDto
            {
                Uln = learner.Uln,
                FirstName = learner.FirstName,
                LastName = learner.LastName,
                DateOfBirth = learner.DateOfBirth
            },
            Episodes = learning.Episodes.Where(e => e.Ukprn == command.Ukprn).Select(e => new ShortCourseEpisodeDto
            {
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                CourseCode = e.TrainingCode,
                CourseType = CourseTypeConstants.ShortCourse,
                LearningType = e.LearningType.ToString(),
                StartDate = e.StartDate,
                AgeAtStart = learner.AgeOnDate(e.StartDate),
                PlannedEndDate = e.ExpectedEndDate,
                WithdrawalDate = e.WithdrawalDate,
                IsApproved = e.IsApproved,
                Price = e.Price,
                LearnerRef = e.LearnerRef
            }).ToArray()
        };
    }
}
