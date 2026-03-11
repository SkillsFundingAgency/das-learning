using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System.Collections.ObjectModel;
using ApprenticeshipLearningEntity = SFA.DAS.Learning.DataAccess.Entities.Learning.ApprenticeshipLearning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ApprenticeshipLearningDomainModel : LearningDomainModel<ApprenticeshipLearningEntity>
{
    private readonly List<ApprenticeshipEpisodeDomainModel> _episodes;

    public Guid Key => _entity.Key;
    public long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public IReadOnlyCollection<ApprenticeshipEpisodeDomainModel> Episodes => new ReadOnlyCollection<ApprenticeshipEpisodeDomainModel>(_episodes);
    public IReadOnlyCollection<EnglishAndMathsDomainModel> EnglishAndMathsCourses => new ReadOnlyCollection<EnglishAndMathsDomainModel>(_entity.EnglishAndMathsCourses.Select(EnglishAndMathsDomainModel.Get).ToList());
    public DateTime StartDate
    {
        get
        {
            var startDate = AllPrices.MinBy(x => x.StartDate)?.StartDate;
            if (startDate == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(StartDate)} could not be found in the {nameof(LearningDomainModel<ApprenticeshipLearningEntity>) }.");
            }

            return startDate.Value;
        }
    }

    public DateTime? EndDate => AllPrices.MaxBy(x => x.StartDate)?.EndDate;
    public IEnumerable<EpisodePriceDomainModel> AllPrices =>
        _episodes.SelectMany(x => x.EpisodePrices);
    public EpisodePriceDomainModel LatestPrice
    {
        get
        {
            var latestPrice = AllPrices.MaxBy(x => x.StartDate);
            if (latestPrice == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(LatestPrice)} could not be found in the {nameof(LearningDomainModel<ApprenticeshipLearningEntity>) }.");
            }

            return latestPrice;
        }
    }
    public ApprenticeshipEpisodeDomainModel LatestEpisode
    {
        get
        {
            var latestEpisode = _episodes.MaxBy(x => x.EpisodePrices.Max(y => y.StartDate));
            if (latestEpisode == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(LatestEpisode)} could not be found in the {nameof(LearningDomainModel<ApprenticeshipLearningEntity>) }.");
            }

            return latestEpisode;
        }
    }

    public int AgeAtStartOfLearning(LearnerModel learnerModel) => learnerModel.DateOfBirth.CalculateAgeAtDate(StartDate);

    internal static ApprenticeshipLearningDomainModel New(long approvalsApprenticeshipId, Guid learnerKey)
    {
        return new ApprenticeshipLearningDomainModel(new ApprenticeshipLearningEntity
        {
            Key = Guid.NewGuid(),
            ApprovalsApprenticeshipId = approvalsApprenticeshipId,
            LearnerKey = learnerKey
        });
    }

    public static ApprenticeshipLearningDomainModel Get(ApprenticeshipLearningEntity entity)
    {
        return new ApprenticeshipLearningDomainModel(entity);
    }

    private ApprenticeshipLearningDomainModel(ApprenticeshipLearningEntity entity): base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes.Select(ApprenticeshipEpisodeDomainModel.Get).ToList();
    }

    public void AddEpisode(
        long ukprn,
        long employerAccountId,
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice,
        decimal? trainingPrice,
        decimal? endpointAssessmentPrice,
        FundingType fundingType,
        FundingPlatform? fundingPlatform,
        long? fundingEmployerAccountId,
        string legalEntityName,
        long? accountLegalEntityId,
        string trainingCode,
        string? trainingCourseVersion)
    {
        var episode = ApprenticeshipEpisodeDomainModel.New(
            ukprn,
            employerAccountId,
            fundingType,
            fundingPlatform,
            fundingEmployerAccountId,
            legalEntityName,
            accountLegalEntityId,
            trainingCode,
            trainingCourseVersion);

        episode.AddEpisodePrice(
            startDate,
            endDate,
            totalPrice,
            trainingPrice,
            endpointAssessmentPrice);

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());
    }

    /// <summary>
    /// This adds a learner updated event which will be emitted by the repository on save.
    /// the current purpose of this event is to trigger history generation
    /// </summary>
    public void AddUpdatedEvent(LearnerUpdatedEvent learnerUpdatedEvent) => AddEvent(learnerUpdatedEvent);

    public Learning.DataAccess.Entities.Learning.ApprenticeshipLearning GetEntity()
    {
        return _entity;
    }


    public LearningUpdateChanges[] Update(LearningUpdateContext updateContext)
    {
        var changes = new List<LearningUpdateChanges>();

        UpdateLearningDetails(updateContext, changes);

        UpdateEnglishAndMathsDetails(updateContext, changes);

        UpdateLearningSupport(updateContext, changes);

        UpdatePrices(updateContext, changes);

        UpdateExpectedEndDate(updateContext, changes);

        UpdateWithdrawalDate(updateContext, changes);

        UpdatePauseDate(updateContext, changes);

        UpdateBreaksInLearning(updateContext, changes);

        return changes.ToArray();
    }

    public void RemoveLearner()
    {
        var latestEpisode = LatestEpisode;
        var withdrawalDate = latestEpisode.EpisodePrices.Min(x => x.StartDate); // This is also the first day of learning
        latestEpisode.Withdraw(withdrawalDate);
        latestEpisode.UpdateLearningSupportIfChanged([]);
        latestEpisode.UpdateBreaksInLearningIfChanged([]);
        _entity.EnglishAndMathsCourses.Clear();
    }

    private void UpdateLearningDetails(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        if (updateModel.Learning.CompletionDate?.Date != _entity.CompletionDate?.Date)
        {
            _entity.CompletionDate = updateModel.Learning.CompletionDate?.Date;
            changes.Add(LearningUpdateChanges.CompletionDate);
        }
    }

    private void UpdateEnglishAndMathsDetails(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        bool hasChanges = false;
        bool hasWithdrawalChanges = false;
        bool hasBreaksInLearningChanges = false;

        var existingCourses = EnglishAndMathsCourses;
        var courseKeysToKeep = new List<Guid>();

        foreach (var incomingCourse in updateModel.EnglishAndMathsCourses)
        {
            var existingCourse = existingCourses.SingleOrDefault(x => x.LearnAimRef.Trim() == incomingCourse.LearnAimRef.Trim());

            if (existingCourse != null)
            {
                courseKeysToKeep.Add(existingCourse.Key);
                hasChanges |= existingCourse.Update(incomingCourse);
                hasWithdrawalChanges |= existingCourse.UpdateWithdrawalDate(incomingCourse);
                hasBreaksInLearningChanges |= existingCourse.UpdateBreaksInLearningIfChanged(incomingCourse.BreaksInLearning);
            }
            else
            {
                hasChanges = true;
                var newCourse = new EnglishAndMathsDomainModel(incomingCourse, _entity.Key);
                _entity.EnglishAndMathsCourses.Add(newCourse.GetEntity());
                if (newCourse.WithdrawalDate.HasValue) hasWithdrawalChanges = true;
                courseKeysToKeep.Add(newCourse.Key);
            }
        }

        var coursesToRemove = _entity.EnglishAndMathsCourses
            .Where(existing => !courseKeysToKeep.Contains(existing.Key))
            .ToList();

        foreach (var removed in coursesToRemove)
        {
            _entity.EnglishAndMathsCourses.Remove(removed);
            hasChanges = true;
        }

        if (hasChanges)
            changes.Add(LearningUpdateChanges.EnglishAndMaths);

        if (hasWithdrawalChanges)
            changes.Add(LearningUpdateChanges.EnglishAndMathsWithdrawal);

        if (hasBreaksInLearningChanges)
            changes.Add(LearningUpdateChanges.EnglishAndMathsBreaksInLearningUpdated);
    }

    private void UpdateLearningSupport(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var learningSupportHasChanged = LatestEpisode.UpdateLearningSupportIfChanged(updateModel.LearningSupport);
        if (learningSupportHasChanged)
        {
            changes.Add(LearningUpdateChanges.LearningSupport);
        }
    }

    private void UpdatePrices(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var hasChanged = LatestEpisode.UpdatePricesIfChanged(updateModel.OnProgrammeDetails.Costs);

        if (hasChanged)
        {
            changes.Add(LearningUpdateChanges.Prices);
        }
    }

    private void UpdateExpectedEndDate(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var hasChanged = LatestEpisode.UpdateExpectedEndDateIfChanged(updateModel.OnProgrammeDetails.ExpectedEndDate);

        if (hasChanged)
        {
            changes.Add(LearningUpdateChanges.ExpectedEndDate);

            var @event = new EndDateChangedEvent
            {
                ApprovalsApprenticeshipId = ApprovalsApprenticeshipId,
                LearningKey = Key,
                PlannedEndDate = EndDate.Value
            };

            AddEvent(@event);
        }
    }

    private void UpdateWithdrawalDate(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var latestEpisode = LatestEpisode;

        if (updateModel.Delivery.WithdrawalDate.HasValue)
        {
            if (updateModel.Delivery.WithdrawalDate == latestEpisode.WithdrawalDate) return;

            latestEpisode.Withdraw(updateModel.Delivery.WithdrawalDate.Value);
            changes.Add(LearningUpdateChanges.Withdrawal);

            var @event = new LearningWithdrawnEvent
            {
                LearningKey = Key,
                ApprovalsApprenticeshipId = ApprovalsApprenticeshipId,
                Reason = latestEpisode.IsWithdrawnBackToStart
                    ? WithdrawReason.WithdrawFromStart.ToString()
                    : WithdrawReason.WithdrawDuringLearning.ToString(),
                LastDayOfLearning = updateModel.Delivery.WithdrawalDate.Value,
                EmployerAccountId = LatestEpisode.EmployerAccountId
            };

            AddEvent(@event);
        }
        else
        {
            if (!latestEpisode.WithdrawalDate.HasValue) return;

            latestEpisode.ReverseWithdrawal();
            changes.Add(LearningUpdateChanges.ReverseWithdrawal);

            var @event = new WithdrawalRevertedEvent
            {
                LearningKey = Key,
                ApprovalsApprenticeshipId = ApprovalsApprenticeshipId
            };

            AddEvent(@event);
        }
    }

    private void UpdatePauseDate(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var latestEpisode = LatestEpisode;

        if (updateModel.OnProgrammeDetails.PauseDate == latestEpisode.PauseDate) return;

        latestEpisode.SetPauseDate(updateModel.OnProgrammeDetails.PauseDate);

        if (updateModel.OnProgrammeDetails.PauseDate.HasValue)
        {
            changes.Add(LearningUpdateChanges.BreakInLearningStarted);
        }
        else
        {
            changes.Add(LearningUpdateChanges.BreakInLearningRemoved);
        }
    }

    private void UpdateBreaksInLearning(LearningUpdateContext updateModel, List<LearningUpdateChanges> changes)
    {
        var breaksInLearningHaveChanged = LatestEpisode.UpdateBreaksInLearningIfChanged(updateModel.OnProgrammeDetails.BreaksInLearning);
        if (breaksInLearningHaveChanged)
        {
            changes.Add(LearningUpdateChanges.BreaksInLearningUpdated);
        }
    }

}