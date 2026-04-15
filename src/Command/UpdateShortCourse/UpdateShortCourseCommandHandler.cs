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
        return new UpdateShortCourseResult
        {
            LearningKey = learning.Key,
            LearnerKey = learning.LearnerKey,
            CompletionDate = learning.CompletionDate,
            Changes = changes,
            Learner = new UpdateShortCourseResultLearner
            {
                Uln = learner.Uln,
                FirstName = learner.FirstName,
                LastName = learner.LastName,
                DateOfBirth = learner.DateOfBirth
            },
            Episodes = learning.Episodes.Where(e => e.Ukprn == command.Model.OnProgramme.Ukprn).Select(e => new UpdateShortCourseResultEpisode
            {
                Ukprn = e.Ukprn,
                EmployerAccountId = e.EmployerAccountId,
                CourseCode = e.TrainingCode,
                CourseType = CourseTypeConstants.ShortCourse,
                LearningType = e.LearningType,
                StartDate = e.StartDate,
                AgeAtStart = learner.AgeOnDate(e.StartDate),
                PlannedEndDate = e.ExpectedEndDate,
                WithdrawalDate = e.WithdrawalDate,
                IsApproved = e.IsApproved,
                Price = e.Price,
                LearnerRef = e.LearnerRef,
                EmployerType = e.EmployerType,
                ApprovalsApprenticeshipId = e.ApprovalsApprenticeshipId,
            }).ToArray()
        };
    }
}
