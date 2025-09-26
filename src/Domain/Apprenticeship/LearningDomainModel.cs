using System.Collections.ObjectModel;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class LearningDomainModel : AggregateRoot
{
    private readonly Learning.DataAccess.Entities.Learning.Learning _entity;
    private readonly List<EpisodeDomainModel> _episodes;
    private readonly List<FreezeRequestDomainModel> _freezeRequests;
    private readonly List<MathsAndEnglishDomainModel> _mathsAndEnglishCourses;

    public Guid Key => _entity.Key;
    public long ApprovalsApprenticeshipId => _entity.ApprovalsApprenticeshipId;
    public string Uln => _entity.Uln;
    public string FirstName => _entity.FirstName;
    public string LastName => _entity.LastName;
    public DateTime DateOfBirth => _entity.DateOfBirth;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public IReadOnlyCollection<EpisodeDomainModel> Episodes => new ReadOnlyCollection<EpisodeDomainModel>(_episodes);
    public IReadOnlyCollection<FreezeRequestDomainModel> FreezeRequests => new ReadOnlyCollection<FreezeRequestDomainModel>(_freezeRequests);
    public IReadOnlyCollection<MathsAndEnglishDomainModel> MathsAndEnglishCourses => new ReadOnlyCollection<MathsAndEnglishDomainModel>(_mathsAndEnglishCourses);
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
    public EpisodeDomainModel LatestEpisode
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

    public int AgeAtStartOfLearning => DateOfBirth.CalculateAgeAtDate(StartDate);

    internal static LearningDomainModel New(
        long approvalsApprenticeshipId,
        string uln,
        DateTime dateOfBirth,
        string firstName,
        string lastName,
        string apprenticeshipHashedId)
    {
        return new LearningDomainModel(new Learning.DataAccess.Entities.Learning.Learning
        {
            Key = Guid.NewGuid(),
            ApprovalsApprenticeshipId = approvalsApprenticeshipId,
            Uln = uln,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            ApprenticeshipHashedId = apprenticeshipHashedId
        });
    }

    public static LearningDomainModel Get(Learning.DataAccess.Entities.Learning.Learning entity)
    {
        return new LearningDomainModel(entity);
    }

    private LearningDomainModel(Learning.DataAccess.Entities.Learning.Learning entity)
    {
        _entity = entity;
        _episodes = entity.Episodes.Select(EpisodeDomainModel.Get).ToList();
        _freezeRequests = entity.FreezeRequests.Select(FreezeRequestDomainModel.Get).ToList();
        _mathsAndEnglishCourses = entity.MathsAndEnglishCourses.Select(MathsAndEnglishDomainModel.Get).ToList();
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
        int fundingBandMaximum,
        long? fundingEmployerAccountId, 
        string legalEntityName, 
        long? accountLegalEntityId,
        string trainingCode,
        string? trainingCourseVersion)
    {
        var episode = EpisodeDomainModel.New(
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
            endpointAssessmentPrice,
            fundingBandMaximum);

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());
    }

    public Learning.DataAccess.Entities.Learning.Learning GetEntity()
    {
        return _entity;
    }

    public void SetPaymentsFrozen(bool newPaymentsFrozenStatus, string userId, DateTime changeDateTime, string? reason = null)
    {
        if (LatestEpisode.PaymentsFrozen == newPaymentsFrozenStatus)
        {
            throw new InvalidOperationException($"Payments are already {(newPaymentsFrozenStatus ? "frozen" : "unfrozen")} for this apprenticeship: {Key}.");
        }

        LatestEpisode.UpdatePaymentStatus(newPaymentsFrozenStatus); 

        if (newPaymentsFrozenStatus)
        {
            var freezeRequest = FreezeRequestDomainModel.New(_entity.Key, userId, changeDateTime, reason);
            _freezeRequests.Add(freezeRequest);
            _entity.FreezeRequests.Add(freezeRequest.GetEntity());
        }
        else
        {
            var freezeRequest = _freezeRequests.Single(x => !x.Unfrozen);
            freezeRequest.Unfreeze(userId, changeDateTime);
        }
    }

    public LearningUpdateChanges[] UpdateLearnerDetails(LearnerUpdateModel updateModel)
    {
        var changes = new List<LearningUpdateChanges>();

        UpdateLearningDetails(updateModel, changes);

        UpdateMathsAndEnglishDetails(updateModel, changes);

        UpdateLearningSupport(updateModel, changes);

        UpdatePrices(updateModel, changes);

        UpdateExpectedEndDate(updateModel, changes);

        return changes.ToArray();
    }

    public void RemoveLearner()
    {
        var latestEpisode = LatestEpisode;
        var lastDayOfLearning = latestEpisode.EpisodePrices.Min(x => x.StartDate); // This is also the first day of learning
        latestEpisode.Withdraw(lastDayOfLearning);
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

        var coursesToAdd = new List<MathsAndEnglish>();
        var courseKeysToKeep = new List<Guid>();

        foreach (var incomingCourse in updateModel.MathsAndEnglishCourses)
        {
            var existingCourse = _entity.MathsAndEnglishCourses.SingleOrDefault(x => 
                x.Course.Trim() == incomingCourse.Course.Trim()
                && x.StartDate == incomingCourse.StartDate
                && x.PlannedEndDate == incomingCourse.PlannedEndDate
                && x.CompletionDate == incomingCourse.CompletionDate
                && x.WithdrawalDate == incomingCourse.WithdrawalDate
                && x.PriorLearningPercentage == incomingCourse.PriorLearningPercentage
                && x.Amount == incomingCourse.Amount);

            if (existingCourse != null)
            {
                courseKeysToKeep.Add(existingCourse.Key);
            }
            else
            {
                coursesToAdd.Add(new MathsAndEnglish
                {
                    Course = incomingCourse.Course,
                    StartDate = incomingCourse.StartDate,
                    PlannedEndDate = incomingCourse.PlannedEndDate,
                    CompletionDate = incomingCourse.CompletionDate,
                    WithdrawalDate = incomingCourse.WithdrawalDate,
                    PriorLearningPercentage = incomingCourse.PriorLearningPercentage,
                    Amount = incomingCourse.Amount
                });
                hasChanges = true;
            }
           
        }

        var coursesToRemove = _entity.MathsAndEnglishCourses
            .Where(existing => !courseKeysToKeep.Any(courseKeyToKeep => courseKeyToKeep == existing.Key))
            .ToList();

        foreach (var removed in coursesToRemove)
        {
            _entity.MathsAndEnglishCourses.Remove(removed);
            _mathsAndEnglishCourses.RemoveAll(x => x.Key == removed.Key);
            hasChanges = true;
        }

        _entity.MathsAndEnglishCourses.AddRange(coursesToAdd);
        _mathsAndEnglishCourses.AddRange(coursesToAdd.Select(MathsAndEnglishDomainModel.Get));

        if (hasChanges) changes.Add(LearningUpdateChanges.MathsAndEnglish);
    }

    private void UpdateLearningSupport(LearnerUpdateModel updateModel, List<LearningUpdateChanges> changes)
    {
        var learningSupportHasChanged = LatestEpisode.UpdateLearningSupportIfChanged(updateModel.LearningSupport);
        if(learningSupportHasChanged)
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

}