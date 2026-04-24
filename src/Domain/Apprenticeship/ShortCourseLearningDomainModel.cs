using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseLearningDomainModel : LearningDomainModel<Learning.DataAccess.Entities.Learning.ShortCourseLearning>
{
    private readonly List<ShortCourseEpisodeDomainModel> _episodes;

    public Guid Key => _entity.Key;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public IReadOnlyCollection<ShortCourseEpisodeDomainModel> Episodes => new ReadOnlyCollection<ShortCourseEpisodeDomainModel>(_episodes);

    internal static ShortCourseLearningDomainModel New(
        Guid learnerKey,
        DateTime? completionDate)
    {
        return new ShortCourseLearningDomainModel(new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = learnerKey,
            CompletionDate = completionDate
        });
    }

    public static ShortCourseLearningDomainModel Get(ShortCourseLearning entity)
    {
        return new ShortCourseLearningDomainModel(entity);
    }

    public Learning.DataAccess.Entities.Learning.ShortCourseLearning GetEntity()
    {
        return _entity;
    }

    public ShortCourseEpisodeDomainModel AddEpisode(
        long ukprn,
        long employerAccountId,
        string learnerRef,
        string trainingCode,
        bool isApproved,
        DateTime startDate,
        DateTime expectedEndDate,
        DateTime? withdrawalDate,
        IEnumerable<Milestone> milestones,
        decimal price = 0,
        LearningType learningType = LearningType.Apprenticeship,
        EmployerType employerType = EmployerType.NonLevy)
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            _entity.Key,
            ukprn,
            employerAccountId,
            learnerRef,
            trainingCode,
            isApproved,
            startDate,
            expectedEndDate,
            withdrawalDate,
            price,
            learningType,
            employerType
        );

        foreach (var milestone in milestones)
        {
            episode.AddMilestone(milestone);
        }

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());

        return episode;
    }

    public ShortCourseUpdateChanges[] Update(ShortCourseUpdateContext updateContext)
    {
        var changes = new List<ShortCourseUpdateChanges>();
        UpdateCompletionDate(updateContext.OnProgramme.CompletionDate, changes);
        UpdateEpisode(updateContext, changes);
        return changes.ToArray();
    }

    private void UpdateCompletionDate(DateTime? completionDate, List<ShortCourseUpdateChanges> changes)
    {
        if (_entity.CompletionDate != completionDate)
            changes.Add(ShortCourseUpdateChanges.CompletionDate);
        _entity.CompletionDate = completionDate;
    }

    private void UpdateEpisode(ShortCourseUpdateContext updateContext, List<ShortCourseUpdateChanges> changes)
    {
        var episode = _episodes.Single();

        var prevWithdrawalDate = episode.WithdrawalDate;
        var prevMilestones = episode.Milestones.Select(m => m.Milestone).ToHashSet();
        var prevLearnerRef = episode.LearnerRef;

        episode.Update(updateContext);

        if (episode.WithdrawalDate != prevWithdrawalDate)
        {
            changes.Add(ShortCourseUpdateChanges.WithdrawalDate);

            if (episode.IsApproved && episode.WithdrawalDate.HasValue)
            {
                AddEvent(new LearningWithdrawnEvent
                {
                    LearningKey = Key,
                    ApprovalsApprenticeshipId = episode.ApprovalsApprenticeshipId,
                    LastDayOfLearning = episode.WithdrawalDate.Value,
                    EmployerAccountId = episode.EmployerAccountId
                });
            }
        }

        if (!episode.Milestones.Select(m => m.Milestone).ToHashSet().SetEquals(prevMilestones))
            changes.Add(ShortCourseUpdateChanges.Milestone);

        if (!episode.LearnerRef.Equals(prevLearnerRef, StringComparison.OrdinalIgnoreCase))
            changes.Add(ShortCourseUpdateChanges.LearnerRef);
    }

    public bool Delete(long ukprn)
    {
        var episode = _episodes.SingleOrDefault(e => e.Ukprn == ukprn);

        if (episode == null || !episode.IsApproved)
            return false;

        episode.Delete();

        AddEvent(new LearningDeletedEvent
        {
            LearningKey = Key,
            ApprovalsApprenticeshipId = episode.ApprovalsApprenticeshipId,
            LastDayOfLearning = episode.StartDate,
            EmployerAccountId = episode.EmployerAccountId
        });

        return true;
    }

    public override void Approve(long employerAccountId)
        => Approve(employerAccountId, EmployerType.NonLevy, 0);

    public void Approve(long employerAccountId, EmployerType employerType, long approvalsApprenticeshipId, long? transferSenderId = null)
    {
        var episode = LatestEpisode;
        episode.Approve(employerAccountId, employerType, approvalsApprenticeshipId, transferSenderId);

        AddEvent(new LearningApprovedEvent
        {
            LearningKey = Key,
            EpisodeKey = episode.Key
        });
    }

    public ShortCourseEpisodeDomainModel LatestEpisode
    {
        get
        {
            var latestEpisode = _episodes.MaxBy(x => x.StartDate);
            return latestEpisode;
        }
    }

    private ShortCourseLearningDomainModel(ShortCourseLearning entity) : base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}
