using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.UpdateShortCourse;

public class UpdateShortCourseCommandHandler(
    ILogger<UpdateShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository)
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

        return new UpdateShortCourseResult { LearningKey = learning.Key, Changes = changes };
    }
}
