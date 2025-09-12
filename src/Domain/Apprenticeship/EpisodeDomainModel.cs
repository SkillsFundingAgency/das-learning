using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Models;
using SFA.DAS.Learning.Enums;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class EpisodeDomainModel
{
    private readonly Episode _entity;
    private readonly List<EpisodePriceDomainModel> _episodePrices;
    public Guid Key => _entity.Key;
    public long Ukprn => _entity.Ukprn;
    public long EmployerAccountId => _entity.EmployerAccountId;
    public FundingType FundingType => _entity.FundingType;
    public FundingPlatform? FundingPlatform => _entity.FundingPlatform;
    public long? FundingEmployerAccountId => _entity.FundingEmployerAccountId;
    public string LegalEntityName => _entity.LegalEntityName;
    public long? AccountLegalEntityId => _entity.AccountLegalEntityId;
    public string TrainingCode => _entity.TrainingCode;
    public string TrainingCourseVersion => _entity.TrainingCourseVersion;
    public bool PaymentsFrozen => _entity.PaymentsFrozen;
    public LearnerStatus LearningStatus => Enum.Parse<LearnerStatus>(_entity.LearningStatus);
    public DateTime? LastDayOfLearning => _entity.LastDayOfLearning;
    public IReadOnlyCollection<LearningSupportDomainModel> LearningSupport => _entity.LearningSupport.SelectOrEmptyList(LearningSupportDomainModel.Get);
    public IReadOnlyCollection<EpisodePriceDomainModel> EpisodePrices => new ReadOnlyCollection<EpisodePriceDomainModel>(_episodePrices);
    public List<EpisodePriceDomainModel> ActiveEpisodePrices => _episodePrices.Where(x => !x.IsDeleted).ToList();
    public EpisodePriceDomainModel LatestPrice
    {
        get
        {
            var latestPrice = _episodePrices.Where(y => !y.IsDeleted).MaxBy(x => x.StartDate);
            if (latestPrice == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(LatestPrice)} could not be found in the {nameof(EpisodeDomainModel)}.");
            }

            return latestPrice;
        }
    }

    public EpisodePriceDomainModel FirstPrice
    {
        get
        {
            var firstPrice = _episodePrices.Where(y => !y.IsDeleted).MinBy(x => x.StartDate);
            if (firstPrice == null)
            {
                throw new InvalidOperationException($"Unexpected error. {nameof(FirstPrice)} could not be found in the {nameof(EpisodeDomainModel)}.");
            }

            return firstPrice;
        }
    }

    internal static EpisodeDomainModel New(
        long ukprn,
        long employerAccountId,
        FundingType fundingType, 
        FundingPlatform? fundingPlatform,
        long? fundingEmployerAccountId, 
        string legalEntityName, 
        long? accountLegalEntityId,
        string trainingCode,
        string? trainingCourseVersion)
    {
        return new EpisodeDomainModel(new Episode
        {
            Ukprn = ukprn,
            EmployerAccountId = employerAccountId,
            FundingType = fundingType,
            FundingPlatform = fundingPlatform,
            FundingEmployerAccountId = fundingEmployerAccountId,
            LegalEntityName = legalEntityName,
            AccountLegalEntityId = accountLegalEntityId,
            TrainingCode = trainingCode,
            TrainingCourseVersion = trainingCourseVersion,
            PaymentsFrozen = false,
            LearningStatus = LearnerStatus.Active.ToString()
        });
    }

    internal void AddEpisodePrice(
        DateTime startDate,
        DateTime endDate,
        decimal totalPrice,
        decimal? trainingPrice,
        decimal? endpointAssessmentPrice,
        int fundingBandMaximum,
        bool shouldSupersedePreviousPrice = false)
    {
        var newEpisodePrice = EpisodePriceDomainModel.New(
            startDate,
            endDate,
            totalPrice,
            trainingPrice,
            endpointAssessmentPrice,
            fundingBandMaximum);

        if (shouldSupersedePreviousPrice)
        {
            LatestPrice.UpdateEndDate(newEpisodePrice.StartDate.AddDays(-1));
        }

        _episodePrices.Add(newEpisodePrice);
        _entity.Prices.Add(newEpisodePrice.GetEntity());
    }

    internal bool UpdatePricesIfChanged(List<Cost> costs)
    {
        var hasChanged = false;

        var existingPrices = _entity.Prices
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.StartDate)
            .ToList();

        var currentPlannedEndDate = existingPrices.LastOrDefault()?.EndDate ?? DateTime.MaxValue;
        var currentFundingBandMaximum = existingPrices.FirstOrDefault()?.FundingBandMaximum ?? default;

        var orderedCosts = costs.OrderBy(x => x.FromDate).ToList();
        var matchedStartDates = new HashSet<DateTime>();

        for (var i = 0; i < orderedCosts.Count; i++)
        {
            var cost = orderedCosts[i];
            var isLast = i == orderedCosts.Count - 1;
            var endDate = isLast ? currentPlannedEndDate : orderedCosts[i + 1].FromDate.AddDays(-1);

            var existing = existingPrices.FirstOrDefault(p =>
                p.StartDate == cost.FromDate);

            if (existing != null)
            {
                matchedStartDates.Add(existing.StartDate);

                if (cost.TrainingPrice != existing.TrainingPrice)
                {
                    existing.TrainingPrice = cost.TrainingPrice;
                    existing.TotalPrice = (existing.TrainingPrice ?? 0) + (existing.EndPointAssessmentPrice ?? 0);
                    hasChanged = true;
                }

                if (cost.EpaoPrice != existing.EndPointAssessmentPrice)
                {
                    existing.EndPointAssessmentPrice = cost.EpaoPrice;
                    existing.TotalPrice = (existing.TrainingPrice ?? 0) + (existing.EndPointAssessmentPrice ?? 0);
                    hasChanged = true;
                }

                if (existing.EndDate != endDate)
                {
                    //sync end date - this does not count as a change
                    //since it must just have been truncated by a subsequent change
                    existing.EndDate = endDate;
                }

                if (existing.FundingBandMaximum != currentFundingBandMaximum)
                {
                    //sync funding band maximum - this does not count as a change
                    //since it might just be due to the first price moving into another band
                    //There is duplication/redundancy in storing FundingBandMaximum on this level since
                    //it applies to the entire episode, not just a price.
                    existing.FundingBandMaximum = currentFundingBandMaximum;
                }

                continue;
            }

            AddEpisodePrice(
                cost.FromDate,
                endDate,
                cost.TotalPrice,
                cost.TrainingPrice,
                cost.EpaoPrice,
                currentFundingBandMaximum,
                false);

            matchedStartDates.Add(cost.FromDate);
            hasChanged = true;
        }

        // Soft-delete unmatched existing prices
        foreach (var price in _entity.Prices.Where(x => !x.IsDeleted))
        {
            if (!matchedStartDates.Contains(price.StartDate))
            {
                price.IsDeleted = true;
                hasChanged = true;
            }
        }

        return hasChanged;
    }

    internal void UpdatePaymentStatus(bool isFrozen)
    {
        _entity.PaymentsFrozen = isFrozen;
    }

    /// <summary>
    /// Updates the learning support if there are differences and returns true. If no differences returns false.
    /// </summary>
    /// <param name="newLearningSupportDetails"></param>
    /// <returns></returns>
    internal bool UpdateLearningSupportIfChanged(List<LearningSupportDetails> newLearningSupportDetails)
    {
        var newLearningSupportRecordsAdded = false;

        //  Remove learning support that are no longer in the new list
        _entity.LearningSupport.RemoveWhere(x=> 
            !newLearningSupportDetails.Any(y => y.StartDate == x.StartDate && y.EndDate == x.EndDate),
            out var removedItems);

        //  Add Learning Support that are in the new list but not in the existing list
        foreach (var newLearningSupport in newLearningSupportDetails)
        {
            if (_entity.LearningSupport.All(x => x.StartDate != newLearningSupport.StartDate || x.EndDate != newLearningSupport.EndDate))
            {
                newLearningSupportRecordsAdded = true;

                _entity.LearningSupport.Add(new LearningSupport
                {
                    StartDate = newLearningSupport.StartDate,
                    EndDate = newLearningSupport.EndDate,
                    LearningKey = _entity.LearningKey
                });
            }
        }

        return newLearningSupportRecordsAdded || removedItems.Count > 0;
    }

    public Episode GetEntity()
    {
        return _entity;
    }

    public static EpisodeDomainModel Get(Episode entity)
    {
        return new EpisodeDomainModel(entity);
    }

    private void DeletePricesStartingAfterDate(DateTime date)
    {
        foreach (var price in _entity.Prices.Where(x => x.StartDate > date && !x.IsDeleted))
        {
            price.IsDeleted = true;
        }
    }

    private void DeletePricesEndingBeforeDate(DateTime date)
    {
        foreach (var price in _entity.Prices.Where(x => x.EndDate < date && !x.IsDeleted))
        {
            price.IsDeleted = true;
        }
    }

    internal void Withdraw(string userId, DateTime lastDateOfLearning)
    {
        _entity.LearningStatus = LearnerStatus.Withdrawn.ToString();
        _entity.LastDayOfLearning = lastDateOfLearning;
    }

    private EpisodeDomainModel(Episode entity)
    {
        _entity = entity;
        _episodePrices = entity.Prices.Select(EpisodePriceDomainModel.Get).ToList();
    }
}

/// <summary>
/// Note there are 2 learner status enums in this repo. (The other is in SFA.DAS.Apprenticeships.Types). The other enum is used for calculated
/// status's and is not persisted. This enum is persisted in the database.
/// </summary>
public enum LearnerStatus
{
    Active,
    Withdrawn
}