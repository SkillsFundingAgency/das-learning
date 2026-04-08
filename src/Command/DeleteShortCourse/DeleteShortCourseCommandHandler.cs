using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Repositories;
using System.Linq;

namespace SFA.DAS.Learning.Command.DeleteShortCourse;

public class DeleteShortCourseCommandHandler(
    ILogger<DeleteShortCourseCommandHandler> logger,
    IShortCourseLearningRepository repository)
    : ICommandHandler<DeleteShortCourseCommand, DeleteShortCourseResult>
{
    public async Task<DeleteShortCourseResult> Handle(DeleteShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Handling DeleteShortCourseCommand for LearningKey {LearningKey}", command.LearningKey);

        var learning = await repository.Get(command.LearningKey);

        if (learning == null)
            throw new KeyNotFoundException($"Short course learning with key {command.LearningKey} not found.");

        var deleted = learning.Delete(command.Ukprn);

        if (!deleted)
        {
            logger.LogInformation("Short course {LearningKey} is not approved; delete request ignored.", command.LearningKey);
            return new DeleteShortCourseResult { WasDeleted = false };
        }

        await repository.Update(learning);
        return new DeleteShortCourseResult { WasDeleted = true };
    }
}
