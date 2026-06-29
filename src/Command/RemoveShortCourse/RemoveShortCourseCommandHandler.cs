using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.RemoveShortCourse;

public class RemoveShortCourseCommandHandler(
    ILogger<RemoveShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository,
    IShortCourseLearningDomainModelMapper mapper)
    : ICommandHandler<RemoveShortCourseCommand, RemoveShortCourseResult?>
{
    public async Task<RemoveShortCourseResult?> Handle(RemoveShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling RemoveShortCourseCommand for LearnerKey {LearnerKey}", command.LearnerKey);

        var learnings = await repository.GetAllByLearnerKey(command.LearnerKey);

        if (learnings.Count == 0)
            throw new NotFoundException($"Short course learning for learner key {command.LearnerKey} not found.");

        var learner = await learnerRepository.Get(command.LearnerKey);
        var ayDates = AcademicYearParser.ParseFrom(command.AcademicYear);

        var result = new RemoveShortCourseResult();

        foreach (var learning in learnings.Where(l => l.Episodes.Any(e =>
            e.Ukprn == command.Ukprn &&
            !e.IsRemoved &&
            e.StartDate <= ayDates.End &&
            (!e.WithdrawalDate.HasValue || e.WithdrawalDate.Value >= ayDates.Start) &&
            (!e.CompletionDate.HasValue || e.CompletionDate.Value >= ayDates.Start))))
        {
            var removedEpisodeKey = learning.Remove(command.Ukprn);
            if (removedEpisodeKey == null) continue;

            await repository.Update(learning);

            var itemResult = mapper.Map<RemoveShortCourseItemResult>(learning, learner!, command.Ukprn);
            itemResult.RemovedEpisodeKey = removedEpisodeKey.Value;
            result.Results.Add(itemResult);
        }

        if (result.Results.Count == 0)
            throw new NotFoundException($"No short course episode found for learner key {command.LearnerKey} and ukprn {command.Ukprn}.");

        return result;
    }
}
