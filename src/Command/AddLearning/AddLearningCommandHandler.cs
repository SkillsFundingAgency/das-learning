using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;

namespace SFA.DAS.Learning.Command.AddLearning;

public class AddLearningCommandHandler : ICommandHandler<AddLearningCommand>
{
    private readonly ILearningService _learningService;
    private readonly ILearnerFactory _learnerFactory;
    private readonly IApprenticeshipLearningFactory _learningFactory;
    private readonly ILearnerRepository _learnerRepository;
    private readonly ILogger<AddLearningCommandHandler> _logger;

    public AddLearningCommandHandler(
        ILearningService learningService,
        ILearnerFactory learnerFactory,
        IApprenticeshipLearningFactory learningFactory,
        ILearnerRepository learnerRepository,
        ILogger<AddLearningCommandHandler> logger)
    {
        _learningService = learningService;
        _learnerFactory = learnerFactory;
        _learningFactory = learningFactory;
        _learnerRepository = learnerRepository;
        _logger = logger;
    }

    public async Task Handle(AddLearningCommand command, CancellationToken cancellationToken = default)
    {
        var existingLearning = await _learningService.GetUnapprovedLearning(command.Uln, command.LearningType, command.ApprovalsApprenticeshipId, command.TrainingCode);

        if (existingLearning != null)
        {
            _logger.LogInformation("Approving unapproved {LearningType} Learning for ULN {Uln}", command.LearningType, command.Uln);

            existingLearning.Approve(new ApproveLearningContext
            {
                Ukprn = command.UKPRN,
                EmployerAccountId = command.EmployerAccountId,
                EmployerType = command.EmployerType,
                ApprovalsApprenticeshipId = command.ApprovalsApprenticeshipId,
                TransferSenderId = command.TransferSenderId,
                LegalEntityName = command.LegalEntityName,
                AccountLegalEntityId = command.AccountLegalEntityId,
                TrainingCourseVersion = command.TrainingCourseVersion
            });
            await _learningService.UpdateLearning(existingLearning);
            return;
        }

        if (command.LearningType == LearningType.ApprenticeshipUnit)
        {
            _logger.LogWarning("Unable to approve ShortCourse for ULN {Uln} - no ShortCourse was found", command.Uln);
            return;
        }

        var learner = await GetOrCreateLearner(command);

        _logger.LogInformation("Handling AddLearningCommand for Approvals Learning Id: {ApprovalsApprenticeshipId}", command.ApprovalsApprenticeshipId);

        var learning = _learningFactory.CreateNew(learner.Key, command.TrainingCode, command.TrainingCourseVersion, command.LearningType);

        learning.AddEpisode(
            command.ApprovalsApprenticeshipId,
            command.UKPRN,
            command.EmployerAccountId,
            command.ActualStartDate ?? command.PlannedStartDate,
            command.PlannedEndDate,
            command.TotalPrice,
            command.TrainingPrice,
            command.EndPointAssessmentPrice,
            command.TransferSenderId,
            command.LegalEntityName,
            command.AccountLegalEntityId,
            command.EmployerType,
            isApproved: true);

        learning.AddEvent(LearnerUpdatedEvent.From(learner, learning));

        await _learningService.AddLearning(learning);
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
}