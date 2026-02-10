using System.Collections.ObjectModel;
using SFA.DAS.Learning.Domain.Enums;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Models.Apprenticeships;
using SFA.DAS.Learning.Enums;
using MathsAndEnglish = SFA.DAS.Learning.DataAccess.Entities.Learning.MathsAndEnglish;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ApprenticeshipLearningDomainModel : LearningDomainModel
{
    private readonly Learning.DataAccess.Entities.Learning.ApprenticeshipLearning _entity;
    private readonly List<ApprenticeshipEpisodeDomainModel> _episodes;
    private readonly List<FreezeRequestDomainModel> _freezeRequests;

    public Guid Key => _entity.Key;
    public long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public LearnerDomainModel Learner => LearnerDomainModel.Get(_entity.Learner);
    public IReadOnlyCollection<ApprenticeshipEpisodeDomainModel> Episodes => new ReadOnlyCollection<ApprenticeshipEpisodeDomainModel>(_episodes);
    public IReadOnlyCollection<FreezeRequestDomainModel> FreezeRequests => new ReadOnlyCollection<FreezeRequestDomainModel>(_freezeRequests);
    public IReadOnlyCollection<MathsAndEnglishDomainModel> MathsAndEnglishCourses => new ReadOnlyCollection<MathsAndEnglishDomainModel>(_entity.MathsAndEnglishCourses.Select(MathsAndEnglishDomainModel.Get).ToList());
    public DateTime StartDate
    {
        get
        {
            var startDate = AllPrices.MinBy(x => x.StartDate)?.StartDate;
            if (startDate == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(StartDate)} could not be found in the {nameof(LearningDomainModel)}.");
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
                throw new InvalidOperationException($"Unexpected error. {nameof(LatestPrice)} could not be found in the {nameof(LearningDomainModel)}.");
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
                throw new InvalidOperationException($"Unexpected error. {nameof(LatestEpisode)} could not be found in the {nameof(LearningDomainModel)}.");
            }

            return latestEpisode;
        }
    }

    public int AgeAtStartOfLearning => Learner.DateOfBirth.CalculateAgeAtDate(StartDate);

    internal static ApprenticeshipLearningDomainModel New(
        long approvalsApprenticeshipId,
        Learning.DataAccess.Entities.Learning.Learner learner)
    {
        return new ApprenticeshipLearningDomainModel(new Learning.DataAccess.Entities.Learning.ApprenticeshipLearning
        {
            Key = Guid.NewGuid(),
            ApprovalsApprenticeshipId = approvalsApprenticeshipId,
            LearnerKey = learner.Key,
            Learner = learner
        });
    }

    public static ApprenticeshipLearningDomainModel Get(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
    {
        return new ApprenticeshipLearningDomainModel(entity);
    }

    private ApprenticeshipLearningDomainModel(Learning.DataAccess.Entities.Learning.ApprenticeshipLearning entity)
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

    public void MarkAsCreated() => AddEvent(this.ToLearnerUpdatedEvent());

    public Learning.DataAccess.Entities.Learning.ApprenticeshipLearning GetEntity()
    {
        return _entity;
    }


    public LearningUpdateChanges[] UpdateLearnerDetails(LearnerUpdateModel updateModel)
    {
        var changes = new List<LearningUpdateChanges>();

        UpdateLearnerDetails(updateModel, changes);

        UpdateLearnerDateOfBirth(updateModel, changes);

        UpdateLearningDetails(updateModel, changes);

        UpdateMathsAndEnglishDetails(updateModel, changes);

        UpdateLearningSupport(updateModel, changes);

        UpdatePrices(updateModel, changes);

        UpdateExpectedEndDate(updateModel, changes);

        UpdateWithdrawalDate(updateModel, changes);

        UpdatePauseDate(updateModel, changes);

        UpdateBreaksInLearning(updateModel, changes);

        UpdateCareDetails(updateModel, changes);

        if (changes.Any()) AddEvent(this.ToLearnerUpdatedEvent());

        return changes.ToArray();
    }

    private void UpdateLearnerDetails(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        //if (updateModel.Learning.FirstName != FirstName || updateModel.Learning.LastName != LastName ||
        //    updateModel.Learning.EmailAddress != EmailAddress)
        //{
        //    _entity.FirstName = updateModel.Learning.FirstName;
        //    _entity.LastName = updateModel.Learning.LastName;
        //    _entity.EmailAddress = updateModel.Learning.EmailAddress;

        //    changes.Add(LearningUpdateChanges.PersonalDetails);

        //    var @event = new PersonalDetailsChangedEvent
        //    {
        //        ApprovalsApprenticeshipId = ApprovalsApprenticeshipId,
        //        LearningKey = Key,
        //        FirstName = FirstName,
        //        LastName = LastName,
        //        EmailAddress = EmailAddress
        //    };

        //    AddEvent(@event);
        //}
    }

    private void UpdateLearnerDateOfBirth(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        //if (updateModel.Learning.DateOfBirth != DateOfBirth)
        //{
        //    _entity.DateOfBirth = updateModel.Learning.DateOfBirth;

        //    changes.Add(LearningUpdateChanges.DateOfBirthChanged);

        //    var @event = new DateOfBirthChangedEvent
        //    {
        //        LearningKey = Key,
        //        DateOfBirth = DateOfBirth
        //    };

        //    AddEvent(@event);
        //}
    }


    public void RemoveLearner()
    {
        var latestEpisode = LatestEpisode;
        var lastDayOfLearning = latestEpisode.EpisodePrices.Min(x => x.StartDate); // This is also the first day of learning
        latestEpisode.Withdraw(lastDayOfLearning);
        latestEpisode.UpdateLearningSupportIfChanged([]);
        latestEpisode.UpdateBreaksInLearningIfChanged([]);
        _entity.MathsAndEnglishCourses.Clear();
    }

    private void UpdateLearningDetails(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        if (updateModel.Learning.CompletionDate?.Date != _entity.CompletionDate?.Date)
        {
            _entity.CompletionDate = updateModel.Learning.CompletionDate?.Date;
            changes.Add(LearningUpdateChanges.CompletionDate);
        }
    }

    private void UpdateMathsAndEnglishDetails(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        bool hasChanges = false;
        bool hasWithdrawalChanges = false;
        bool hasBreaksInLearningChanges = false;

        var existingCourses = MathsAndEnglishCourses;
        var courseKeysToKeep = new List<Guid>();

        foreach (var incomingCourse in updateModel.MathsAndEnglishCourses)
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
                var newCourse = new MathsAndEnglishDomainModel(incomingCourse, _entity.Key);
                _entity.MathsAndEnglishCourses.Add(newCourse.GetEntity());
                if (newCourse.WithdrawalDate.HasValue) hasWithdrawalChanges = true;
                courseKeysToKeep.Add(newCourse.Key);
            }
        }

        var coursesToRemove = _entity.MathsAndEnglishCourses
            .Where(existing => !courseKeysToKeep.Contains(existing.Key))
            .ToList();

        foreach (var removed in coursesToRemove)
        {
            _entity.MathsAndEnglishCourses.Remove(removed);
            hasChanges = true;
        }

        if (hasChanges)
            changes.Add(LearningUpdateChanges.MathsAndEnglish);

        if (hasWithdrawalChanges)
            changes.Add(LearningUpdateChanges.MathsAndEnglishWithdrawal);

        if (hasBreaksInLearningChanges)
            changes.Add(LearningUpdateChanges.EnglishAndMathsBreaksInLearningUpdated);
    }

    private void UpdateLearningSupport(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        var learningSupportHasChanged = LatestEpisode.UpdateLearningSupportIfChanged(updateModel.LearningSupport);
        if (learningSupportHasChanged)
        {
            changes.Add(LearningUpdateChanges.LearningSupport);
        }
    }

    private void UpdatePrices(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        var hasChanged = LatestEpisode.UpdatePricesIfChanged(updateModel.OnProgrammeDetails.Costs);

        if (hasChanged)
        {
            changes.Add(LearningUpdateChanges.Prices);
        }
    }

    private void UpdateExpectedEndDate(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
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

    private void UpdateWithdrawalDate(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        var latestEpisode = LatestEpisode;

        if (updateModel.Delivery.WithdrawalDate.HasValue)
        {
            if (updateModel.Delivery.WithdrawalDate == latestEpisode.LastDayOfLearning) return;

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
            if (!latestEpisode.LastDayOfLearning.HasValue) return;

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

    private void UpdatePauseDate(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
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

    private void UpdateBreaksInLearning(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        var breaksInLearningHaveChanged = LatestEpisode.UpdateBreaksInLearningIfChanged(updateModel.OnProgrammeDetails.BreaksInLearning);
        if (breaksInLearningHaveChanged)
        {
            changes.Add(LearningUpdateChanges.BreaksInLearningUpdated);
        }
    }

    private void UpdateCareDetails(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        //if (_entity.HasEHCP != updateModel.Learning.Care.HasEHCP ||
        //    _entity.IsCareLeaver != updateModel.Learning.Care.IsCareLeaver ||
        //    _entity.CareLeaverEmployerConsentGiven != updateModel.Learning.Care.CareLeaverEmployerConsentGiven)
        //{
        //    _entity.HasEHCP = updateModel.Learning.Care.HasEHCP;
        //    _entity.IsCareLeaver = updateModel.Learning.Care.IsCareLeaver;
        //    _entity.CareLeaverEmployerConsentGiven = updateModel.Learning.Care.CareLeaverEmployerConsentGiven;
        //    changes.Add(LearningUpdateChanges.Care);
        //}
    }
}