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
        logger.LogInformation("Handling RemoveShortCourseCommand for LearningKey {LearningKey}", command.LearningKey);

        var learning = await repository.Get(command.LearningKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning with key {command.LearningKey} not found.");

        var removed = learning.Remove(command.Ukprn);

        if (!removed)
        {
            logger.LogInformation("Short course {LearningKey} is not approved; remove request ignored.", command.LearningKey);
            return null;
        }

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<RemoveShortCourseResult>(learning, learner!, command.Ukprn);
        return result;
    }
}
