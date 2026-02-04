using System;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Command.RemoveLearnerCommand;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Models.ShortCourses;
using SFA.DAS.Learning.Domain.Repositories;

namespace SFA.DAS.Learning.Command.CreateDraftShortCourse;

public class CreateDraftShortCourseCommand : ICommand
{
    public CreateDraftShortCourseCommand(CreateDraftShortCourseModel model)
    {
        Model = model;
    }

    public CreateDraftShortCourseModel Model { get; }
}

public class CreateDraftShortCourseResult
{
    public Guid LearningKey { get; set; }
}

public class CreateDraftShortCourseCommandHandler : ICommandHandler<CreateDraftShortCourseCommand, CreateDraftShortCourseResult>
{
    private readonly ILearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<CreateDraftShortCourseCommandHandler> _logger;

    public CreateDraftShortCourseCommandHandler(
        ILearningRepository learningRepository,
        IMessageSession messageSession,
        ILogger<CreateDraftShortCourseCommandHandler> logger)
    {
        _learningRepository = learningRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task<CreateDraftShortCourseResult> Handle(CreateDraftShortCourseCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling CreateDraftShortCourseCommand");

        //var existingLearning = await _learningRepository.Get(command.Uln, command.ApprovalsApprenticeshipId);
        //if (existingLearning != null)
        //{
        //    _logger.LogInformation("Learning not created as a record already exists with given ULN and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.", command.ApprovalsApprenticeshipId);
        //    return;
        //}

        return new CreateDraftShortCourseResult() {  }; //todo populate LearningKey
    }

    //private async Task SendEvent(LearningDomainModel learning, DateTime lastDayOfLearning)
    //{
    //    _logger.LogInformation("Publishing ApprenticeshipWithdrawnEvent for {learningKey}", learning.Key);
    //    var message = new LearningWithdrawnEvent
    //    {
    //        LearningKey = learning.Key,
    //        ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
    //        Reason = WithdrawReason.WithdrawFromStart.ToString(),
    //        LastDayOfLearning = lastDayOfLearning,
    //        EmployerAccountId = learning.LatestEpisode.EmployerAccountId
    //    };

    //    await _messageSession.Publish(message);
    //}
}