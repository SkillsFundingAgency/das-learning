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
    public IReadOnlyCollection<ShortCourseEpisodeDomainModel> Episodes => new ReadOnlyCollection<ShortCourseEpisodeDomainModel>(_episodes);

    internal static ShortCourseLearningDomainModel New(Guid learnerKey)
    {
        return new ShortCourseLearningDomainModel(new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = learnerKey
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
        short? withdrawalReasonCode,
        IEnumerable<Milestone> milestones,
        decimal price = 0,
        LearningType learningType = LearningType.Apprenticeship,
        EmployerType employerType = EmployerType.NonLevy,
        DateTime? completionDate = null)
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
            withdrawalReasonCode,
            price,
            learningType,
            employerType,
            completionDate
        );

        foreach (var milestone in milestones)
        {
            episode.AddMilestone(milestone);
        }

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());

        return episode;
    }

    public ShortCourseDomainUpdateResult Update(ShortCourseUpdateContext updateContext)
    {
        var changes = new List<ShortCourseUpdateChanges>();
        var episode = _episodes.Single(e => e.Ukprn == updateContext.OnProgramme.Ukprn);
        ReinstateIfRemoved(episode, changes);
        UpdateEpisode(episode, updateContext, changes);
        return new ShortCourseDomainUpdateResult
        {
            EpisodeKey = episode.Key,
            Changes = changes.ToArray()
        };
    }

    private void ReinstateIfRemoved(ShortCourseEpisodeDomainModel episode, List<ShortCourseUpdateChanges> changes)
    {
        if (!episode.IsApproved || !episode.IsRemoved)
            return;

        episode.Reinstate();

        AddEvent(new LearningReinstatedEvent
        {
            LearningKey = Key,
            ApprenticeshipId = episode.ApprovalsApprenticeshipId
        });

        changes.Add(ShortCourseUpdateChanges.Reinstated);
    }

    private void UpdateEpisode(ShortCourseEpisodeDomainModel episode, ShortCourseUpdateContext updateContext, List<ShortCourseUpdateChanges> changes)
    {
        var prevWithdrawalDate = episode.WithdrawalDate;
        var prevCompletionDate = episode.CompletionDate;
        var prevMilestones = episode.Milestones.Select(m => m.Milestone).ToHashSet();
        var prevLearnerRef = episode.LearnerRef;

        episode.Update(updateContext);

        if (episode.CompletionDate != prevCompletionDate)
            changes.Add(ShortCourseUpdateChanges.CompletionDate);

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
                    WithdrawalReasonCode = episode.WithdrawalReason ?? 0,
                    Created = DateTime.UtcNow,
                    EmployerAccountId = episode.EmployerAccountId
                });
            }
        }

        if (!episode.Milestones.Select(m => m.Milestone).ToHashSet().SetEquals(prevMilestones))
            changes.Add(ShortCourseUpdateChanges.Milestone);

        if (!episode.LearnerRef.Equals(prevLearnerRef, StringComparison.OrdinalIgnoreCase))
            changes.Add(ShortCourseUpdateChanges.LearnerRef);
    }

    public Guid? Remove(long ukprn)
    {
        var episode = _episodes.SingleOrDefault(e => e.Ukprn == ukprn);

        if (episode == null || !episode.IsApproved)
            return null;

        episode.Remove();

        AddEvent(new LearningRemovedEvent
        {
            LearningKey = Key,
            ApprenticeshipId = episode.ApprovalsApprenticeshipId
        });
        return episode.Key;
    }

    public override void Approve(long ukprn, long employerAccountId)
        => Approve(ukprn, employerAccountId, EmployerType.NonLevy, 0);

    public void Approve(long ukprn, long employerAccountId, EmployerType employerType, long approvalsApprenticeshipId, long? transferSenderId = null)
    {
        var episode = LatestEpisodeForProvider(ukprn);
        episode.Approve(employerAccountId, employerType, approvalsApprenticeshipId, transferSenderId);

        AddEvent(new LearningApprovedEvent
        {
            LearningKey = Key,
            EpisodeKey = episode.Key
        });
    }

    public ShortCourseEpisodeDomainModel LatestEpisodeForProvider(long ukprn)
    {
        var latestEpisode = _episodes.Where(x => x.Ukprn == ukprn).MaxBy(x => x.StartDate);
        return latestEpisode;
    }

    private ShortCourseLearningDomainModel(ShortCourseLearning entity) : base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}
