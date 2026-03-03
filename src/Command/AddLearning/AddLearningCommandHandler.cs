using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Builders;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.AddLearning;

public class AddLearningCommandHandler : ICommandHandler<AddLearningCommand>
{
    private readonly ILearningService _learningService;
    private readonly ILearnerFactory _learnerFactory;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly IApprenticeshipLearningRepository _learningRepository;
    private readonly IMessageSession _messageSession;
    private readonly ILogger<AddLearningCommandHandler> _logger;

    public AddLearningCommandHandler(
        ILearningService learningService,
        ILearnerFactory learnerFactory,
        IApprenticeshipLearningFactory learningFactory,
        ILearnerRepository learnerRepository,
        IApprenticeshipLearningRepository learningRepository,
        IMessageSession messageSession,
        ILogger<AddLearningCommandHandler> logger)
    {
        _learningService = learningService;
        _learnerFactory = learnerFactory;
        _learningFactory = learningFactory;
        _learnerRepository = learnerRepository;
        _learningRepository = learningRepository;
        _messageSession = messageSession;
        _logger = logger;
    }

    public async Task Handle(AddLearningCommand command, CancellationToken cancellationToken = default)
    {
        /*
         * ApprenticeshipCreatedEventHandler will be updated to first check if an unapproved learning record with a LearningType matching the learning type on the event.
            If an approved record with the same learning type exists the handler will exit
           If an unapproved record with the same learning type exists then the handler will set the IsApproved field to true and publish a new 
            Apprenticeship Units - Approval | LearningApprovedEvent will be published.
           If no matching record exists (this should not happen for Short Courses)
           then the Learning will be created using the data from approvals as per the current process.
         */

        //get by uln and type? and, if apprenticeship, the ApprovalsApprenticeshipId? and by status? so that we can get unapproved only?
        //would also be great if it could return a base type or something...?
        //so get by ULN, type, and Unapproved = false (plus optional approvalsApprenticeshipId)
        //if any result, set IsApproved to true and exit (this will emit a new event)
        //otherwise... if shortcourse, log error and exit, if apprenticeship AddLearning as below

        //could I do an .Exists() instead? is that really what is needed? and if true, and is ShortCourse, then in another repo call Get the shortcourse
        //or create a new interface, ILearning, that both apprenticeships and shortcourses implement. we could then get that from a new repo/service

        var existingLearning = await _learningService.GetLearning(command.Uln, command.LearningType, false, command.ApprovalsApprenticeshipId);

        if (existingLearning != null && command.LearningType == LearningType.ApprenticeshipUnit)
        {
            _logger.LogInformation($"Approving unapproved ShortCourse for ULN {command.Uln}");

            existingLearning.Approve();
            await _learningService.UpdateLearning(existingLearning, LearningType.ApprenticeshipUnit);
            return;
        }

        if (command.LearningType == LearningType.ApprenticeshipUnit)
        {
            _logger.LogWarning($"Unable to approve ShortCourse for ULN {command.Uln} - no ShortCourse was found");
            return;
        }

        //By the time we get to this line, we are only dealing with Apprenticeships (and Foundation Apprenticeships)

        if (existingLearning != null)
        {
            _logger.LogInformation("Learning not created as a record already exists with given ULN and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.", command.ApprovalsApprenticeshipId);
            return;
        }

        //By the time we get to this line, we are dealing with Apprenticeships that don't exist
        //So we fall into our original behaviour of creating it

        var learner = await GetOrCreateLearner(command);

        _logger.LogInformation("Handling AddLearningCommand for Approvals Learning Id: {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        var learning = _learningFactory.CreateNew(command.ApprovalsApprenticeshipId, learner.Key);

        learning.AddEpisode(
            command.UKPRN,
            command.EmployerAccountId,
            command.ActualStartDate ?? command.PlannedStartDate,
            command.PlannedEndDate,
            command.TotalPrice,
            command.TrainingPrice,
            command.EndPointAssessmentPrice,
            command.FundingType,
            command.FundingPlatform,
            command.FundingEmployerAccountId,
            command.LegalEntityName,
            command.AccountLegalEntityId,
            command.TrainingCode,
            command.TrainingCourseVersion);

        var eventBuilder = new LearnerUpdatedEventBuilder(learner,learning);
        learning.AddUpdatedEvent(eventBuilder.CreateEvent());

        try
        {
            await _learningRepository.Add(learning); //todo: can we use the abstract repo service to add this? I think... so?
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2627 or 2601 })
        {
            //2627: violation of unique constraint. 2601: violation of unique index
            _logger.LogWarning(
                "Unique constraint violation for given Uln and ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.",
                command.ApprovalsApprenticeshipId);
            return;
        }

        if (learning.LatestEpisode.FundingPlatform == FundingPlatform.DAS)
        {
            await SendEvent(learning, learner);
        }
    }

    private async Task<LearnerDomainModel> GetOrCreateLearner(AddLearningCommand command)
    {
        var learner = await _learnerRepository.GetByUln(command.Uln);

        if (learner != null)
        {
            return learner;
        }

        var newLearner = _learnerFactory.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName);
        await _learnerRepository.Add(newLearner);
        return newLearner;
    }

    private async Task SendEvent(ApprenticeshipLearningDomainModel learning, LearnerDomainModel learner)
    {

        _logger.LogInformation(
            "Sending LearningCreatedEvent for ApprovalsApprenticeshipId: {ApprovalsApprenticeshipId}.",
            learning.ApprovalsApprenticeshipId);
        var learningCreatedEvent = new LearningCreatedEvent
        {
            LearningKey = learning.Key,
            Uln = learner.Uln,
            ApprovalsApprenticeshipId = learning.ApprovalsApprenticeshipId,
            DateOfBirth = learner.DateOfBirth,
            FirstName = learner.FirstName,
            LastName = learner.LastName,
            Episode = learning.BuildEpisodeForIntegrationEvent(learner)
        };

        await _messageSession.Publish(learningCreatedEvent);
    }
}