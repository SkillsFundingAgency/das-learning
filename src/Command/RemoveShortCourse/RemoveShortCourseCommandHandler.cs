using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
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

        var learning = await repository.GetByLearnerKey(command.LearnerKey);

        if (learning == null)
            throw new NotFoundException($"Short course learning for learner key {command.LearnerKey} not found.");

        var removedEpisodeKey = learning.Remove(command.Ukprn);

        if (removedEpisodeKey == null)
            throw new NotFoundException($"No approved short course episode found for learner key {command.LearnerKey} and ukprn {command.Ukprn}.");

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<RemoveShortCourseResult>(learning, learner!, command.Ukprn);
        result?.RemovedEpisodeKey = removedEpisodeKey.Value;
        return result;
    }
}
