using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Models.Dtos;

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
        logger.LogInformation("Handling UpdateShortCourseCommand for LearningKey {LearningKey}", command.LearningKey);

        var learning = await repository.Get(command.LearningKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning with key {command.LearningKey} not found.");

        var changes = learning.Update(command.Model);

        await repository.Update(learning);

        var learner = await learnerRepository.Get(learning.LearnerKey);

        var result = mapper.Map<UpdateShortCourseResult>(learning, learner!, command.Model.OnProgramme.Ukprn);
        result.Changes = changes;
        return result;
    }
}
