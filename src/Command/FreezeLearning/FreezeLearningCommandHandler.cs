using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Learning.Command.FreezeLearning;

public class FreezeLearningCommandHandler : ICommandHandler<FreezeLearningCommand>
{

    private readonly ILogger<FreezeLearningCommandHandler> _logger;

    public FreezeLearningCommandHandler(ILogger<FreezeLearningCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(FreezeLearningCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling FreezeLearningCommand");
        return Task.CompletedTask;
    }
}
