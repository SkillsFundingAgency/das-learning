using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.Configuration;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommandHandler(
    ILogger<UpdateShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository,
    IShortCourseLearningDomainModelMapper mapper,
    FeatureFlags featureFlags,
    IShortCourseLearningFactory? factory = null)
    : ICommandHandler<UpdateShortCourseCommand, UpdateShortCourseResult>
{
    public async Task<UpdateShortCourseResult> Handle(UpdateShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateShortCourseCommand for LearnerKey {LearnerKey}", command.LearnerKey);

        var learning = await repository.GetByLearnerKeyAndCourseCode(command.LearnerKey, command.Model.OnProgramme.CourseCode);

        if (learning == null)
        {
            if (!featureFlags.ShortCourseProgression)
            {
                logger.LogInformation(
                    "No learning found for LearnerKey {LearnerKey} / CourseCode {CourseCode} and ShortCourseProgression is disabled — ignoring",
                    command.LearnerKey, command.Model.OnProgramme.CourseCode);
                return new UpdateShortCourseResult { IsIgnored = true };
            }

            return await CreateNewLearning(command);
        }

        var updateResult = learning.Update(command.Model);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, command.Model.OnProgramme.Ukprn);
        result.Changes = updateResult.Changes;
        result.UpdatedEpisodeKey = updateResult.EpisodeKey;
        return result;
    }

    private async Task<UpdateShortCourseResult> CreateNewLearning(UpdateShortCourseCommand command)
    {
        var op = command.Model.OnProgramme;

        var learning = factory!.CreateNew(command.LearnerKey, op.CourseCode);

        var episode = learning.AddEpisode(
            op.Ukprn,
            op.EmployerId,
            command.Model.LearnerRef,
            op.CourseCode,
            false,
            op.StartDate,
            op.ExpectedEndDate,
            op.WithdrawalDate,
            op.Milestones,
            op.Price,
            op.LearningType,
            completionDate: op.CompletionDate);

        foreach (var ls in command.Model.LearningSupport)
            episode.AddLearningSupport(ls.StartDate, ls.EndDate);

        await repository.Add(learning);

        var learner = await learnerRepository.Get(command.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, op.Ukprn);
        result.UpdatedEpisodeKey = episode.Key;
        result.IsNewLearning = true;
        return result;
    }
}
