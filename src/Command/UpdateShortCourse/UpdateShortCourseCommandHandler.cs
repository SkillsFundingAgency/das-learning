using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommandHandler(
    ILogger<UpdateShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository,
    ILearnerRepository learnerRepository,
    IShortCourseLearningDomainModelMapper mapper)
    : ICommandHandler<UpdateShortCourseCommand, UpdateShortCourseResult>
{

    public async Task<UpdateShortCourseResult> Handle(UpdateShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling UpdateShortCourseCommand for LearnerKey {LearnerKey}", command.LearnerKey);

        var learning = await repository.GetByLearnerKey(command.LearnerKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning for learner key {command.LearnerKey} not found.");

        var updateResult = learning.Update(command.Model);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, command.Model.OnProgramme.Ukprn);
        if (result != null)
        {
            result.Changes = updateResult.Changes;
            result.UpdatedEpisodeKey = updateResult.EpisodeKey;
        }
        return result;
    }
}
