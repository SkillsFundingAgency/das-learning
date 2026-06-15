using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommandHandler(
    ILogger<UpdateShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository,
    IShortCourseLearningDomainModelMapper mapper,
    FeatureFlags featureFlags,
    IShortCourseLearningFactory? factory = null)
    : ICommandHandler<UpdateShortCourseCommand, List<UpdateShortCourseResult>>
{
    public async Task<List<UpdateShortCourseResult>> Handle(UpdateShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateShortCourseCommand for LearnerKey {LearnerKey}", command.LearnerKey);

        var results = new List<UpdateShortCourseResult>();
        foreach (var model in command.Models)
        {
            results.Add(await HandleSingleItem(command.LearnerKey, model));
        }

        return results;
    }

    private async Task<UpdateShortCourseResult> HandleSingleItem(Guid learnerKey, ShortCourseUpdateContext model)
    {
        var learning = await repository.GetByLearnerKeyAndCourseCode(learnerKey, model.OnProgramme.CourseCode);

        if (learning == null)
        {
            if (!featureFlags.ShortCourseProgression)
            {
                logger.LogInformation(
                    "No learning found for LearnerKey {LearnerKey} / CourseCode {CourseCode} and ShortCourseProgression is disabled — ignoring",
                    learnerKey, model.OnProgramme.CourseCode);
                return new UpdateShortCourseResult { IsIgnored = true };
            }

            return await CreateNewLearning(learnerKey, model);
        }

        var updateResult = learning.Update(model);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, model.OnProgramme.Ukprn);
        result.CourseCode = model.OnProgramme.CourseCode;
        result.Changes = updateResult.Changes;
        result.UpdatedEpisodeKey = updateResult.EpisodeKey;
        return result;
    }

    private async Task<UpdateShortCourseResult> CreateNewLearning(Guid learnerKey, ShortCourseUpdateContext model)
    {
        var op = model.OnProgramme;

        var learning = factory!.CreateNew(learnerKey, op.CourseCode);

        var episode = learning.AddEpisode(
            op.Ukprn,
            op.EmployerId,
            model.LearnerRef,
            op.CourseCode,
            false,
            op.StartDate,
            op.ExpectedEndDate,
            op.WithdrawalDate,
            op.Milestones,
            op.Price,
            op.LearningType,
            completionDate: op.CompletionDate);

        foreach (var ls in model.LearningSupport)
            episode.AddLearningSupport(ls.StartDate, ls.EndDate);

        await repository.Add(learning);

        var learner = await learnerRepository.Get(learnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, op.Ukprn);
        result.CourseCode = op.CourseCode;
        result.UpdatedEpisodeKey = episode.Key;
        result.IsNewLearning = true;
        return result;
    }
}
