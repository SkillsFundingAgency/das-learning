using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Apprenticeship;
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
    : ICommandHandler<UpdateShortCourseCommand, UpdateShortCourseResponse>
{
    public async Task<UpdateShortCourseResponse> Handle(UpdateShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateShortCourseCommand for LearnerKey {LearnerKey}", command.LearnerKey);

        var results = new List<UpdateShortCourseResult>();
        var processedLearningKeys = new HashSet<Guid>();

        foreach (var model in command.Models)
        {
            var result = await HandleSingleItem(command.LearnerKey, model);
            results.Add(result);
            if (!result.IsIgnored || result.LearningKey != Guid.Empty)
            {
                processedLearningKeys.Add(result.LearningKey);
            }
        }

        if (featureFlags.ShortCourseProgression)
        {
            await RemoveOmittedLearnings(command, results, processedLearningKeys);
        }

        return new UpdateShortCourseResponse { Results = results };
    }

    private async Task RemoveOmittedLearnings(UpdateShortCourseCommand command, List<UpdateShortCourseResult> results, HashSet<Guid> processedLearningKeys)
    {
        var allLearnings = await repository.GetAllByLearnerKey(command.LearnerKey);

        foreach (var learning in allLearnings.Where(l => !processedLearningKeys.Contains(l.Key)))
        {
            var activeEpisode = learning.Episodes.SingleOrDefault(e => e.Ukprn == command.Ukprn && !e.IsRemoved);
            if (activeEpisode == null)
                continue;

            var removedEpisodeKey = learning.Remove(command.Ukprn);

            if (!removedEpisodeKey.HasValue)
                continue;

            await repository.Update(learning);

            logger.LogInformation("Removed omitted Learning {LearningKey} / {CourseCode} for LearnerKey {LearnerKey}",
                learning.Key, learning.TrainingCode, command.LearnerKey);

            results.Add(new UpdateShortCourseResult
            {
                IsRemoved = true,
                LearningKey = learning.Key,
                CourseCode = learning.TrainingCode,
                UpdatedEpisodeKey = removedEpisodeKey.Value
            });
        }
    }

    private async Task<UpdateShortCourseResult> HandleSingleItem(Guid learnerKey, ShortCourseUpdateContext model)
    {
        var learning = await repository.GetByLearnerKeyAndCourseCode(learnerKey, model.OnProgramme.CourseCode);

        if (learning == null)
        {
            if (!featureFlags.ShortCourseProgression)
            {
                logger.LogInformation(
                    "No learning found for LearnerKey {LearnerKey} / CourseCode {CourseCode} and Short Course Progression is disabled — ignoring",
                    learnerKey, model.OnProgramme.CourseCode);
                return new UpdateShortCourseResult { IsIgnored = true };
            }

            return await CreateNewLearning(learnerKey, model);
        }

        var ukprn = model.OnProgramme.Ukprn;
        var existingEpisodeForProvider = learning.Episodes.Any(x => x.Ukprn == ukprn);

        if (!existingEpisodeForProvider)
        {
            if (!featureFlags.ShortCourseChangeOfProvider)
            {
                logger.LogInformation(
                    "Learning found for LearnerKey {LearnerKey} / CourseCode {CourseCode} but the existing episode belongs to a different provider; Short Course Change of Provider is disabled — ignoring",
                    learnerKey, model.OnProgramme.CourseCode);
                return new UpdateShortCourseResult { IsIgnored = true };
            }

            return await AddEpisodeToExistingLearning(learning, model);
        }

        var episodeForProvider = learning.Episodes.Single(x => x.Ukprn == ukprn);
        if (episodeForProvider.HasActualEndDate)
        {
            logger.LogInformation(
                "Episode for LearnerKey {LearnerKey} / CourseCode {CourseCode} / Provider {Ukprn} has actual end date (withdrawn or completed) and Restarts are not supported - ignoring",
                learnerKey, model.OnProgramme.CourseCode, ukprn);
            return new UpdateShortCourseResult { IsIgnored = true, LearningKey = learning.Key };
        }

        var updateResult = learning.Update(model);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, ukprn);
        result.CourseCode = model.OnProgramme.CourseCode;
        result.Changes = updateResult.Changes;
        result.UpdatedEpisodeKey = updateResult.EpisodeKey;
        return result;
    }

    private async Task<UpdateShortCourseResult> AddEpisodeToExistingLearning(ShortCourseLearningDomainModel learning, ShortCourseUpdateContext model)
    {
        var op = model.OnProgramme;

        var episode = learning.AddEpisode(
            op.Ukprn,
            op.EmployerId,
            model.LearnerRef,
            op.CourseCode,
            false,
            op.StartDate,
            op.ExpectedEndDate,
            op.WithdrawalDate,
            op.WithdrawalReasonCode,
            op.Milestones,
            op.Price,
            op.LearningType,
            completionDate: op.CompletionDate);

        foreach (var ls in model.LearningSupport)
            episode.AddLearningSupport(ls.StartDate, ls.EndDate);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, op.Ukprn);
        result.CourseCode = op.CourseCode;
        result.UpdatedEpisodeKey = episode.Key;
        result.IsNewEpisode = true;
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
            op.WithdrawalReasonCode,
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
