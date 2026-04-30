using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Models.UpdateModels;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class EnglishAndMathsDomainModel
{
    private readonly EnglishAndMaths _entity;

    public Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public string LearnAimRef => _entity.LearnAimRef;
    public DateTime StartDate => _entity.StartDate;
    public DateTime PlannedEndDate => _entity.PlannedEndDate;
    public string Course => _entity.Course;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public DateTime? PauseDate => _entity.PauseDate;
    public int? CombinedFundingAdjustmentPercentage => _entity.CombinedFundingAdjustmentPercentage;
    public decimal Amount => _entity.Amount;
    public IReadOnlyCollection<EnglishAndMathsBreakInLearningDomainModel> BreaksInLearning => new ReadOnlyCollection<EnglishAndMathsBreakInLearningDomainModel>(_entity.BreaksInLearning.Select(EnglishAndMathsBreakInLearningDomainModel.Get).ToList());


    internal EnglishAndMathsDomainModel(EnglishAndMaths entity)
    {
        _entity = entity;
    }

    public EnglishAndMathsDomainModel(EnglishAndMathsUpdateDetails incomingCourse, Guid learningKey)
    {
        _entity = new EnglishAndMaths
        {
            Key = Guid.NewGuid(),
            LearningKey = learningKey,
            LearnAimRef = incomingCourse.LearnAimRef,
            Course = incomingCourse.Course,
            StartDate = incomingCourse.StartDate,
            PlannedEndDate = incomingCourse.PlannedEndDate,
            CompletionDate = incomingCourse.CompletionDate,
            PauseDate = incomingCourse.PauseDate,
            Amount = incomingCourse.Amount,
            CombinedFundingAdjustmentPercentage = incomingCourse.CombinedFundingAdjustmentPercentage,
            WithdrawalDate = incomingCourse.WithdrawalDate
        };

        _entity.BreaksInLearning = incomingCourse.BreaksInLearning.Select(b => new DataAccess.Entities.Learning.EnglishAndMathsBreakInLearning
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = _entity.Key,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            PriorPeriodExpectedEndDate = b.PriorPeriodExpectedEndDate
        }).ToList();
    }

    public static EnglishAndMathsDomainModel Get(EnglishAndMaths entity)
    {
        return new EnglishAndMathsDomainModel(entity);
    }

    /// <summary>
    /// Updates the breaks in learning if there are differences and returns true; if no differences returns false.
    /// </summary>
    /// <param name="newBreaksInLearning">The new breaks in learning</param>
    /// <returns>True if differences, otherwise false</returns>
    internal bool UpdateBreaksInLearningIfChanged(List<BreakInLearningUpdateDetails> newBreaksInLearning)
    {
        var hasBreaksInLearningChanged = false;

        //  Remove breaks in learning that are no longer in the new list
        _entity.BreaksInLearning.RemoveWhere(x =>
                !newBreaksInLearning.Any(y => y.StartDate == x.StartDate && y.EndDate == x.EndDate && y.PriorPeriodExpectedEndDate == x.PriorPeriodExpectedEndDate),
            out var removedItems);

        //  Add breaks in learning that are in the new list but not in the existing list
        foreach (var newBreakInLearning in newBreaksInLearning)
        {
            if (_entity.BreaksInLearning.All(x => x.StartDate != newBreakInLearning.StartDate || x.EndDate != newBreakInLearning.EndDate || x.PriorPeriodExpectedEndDate != newBreakInLearning.PriorPeriodExpectedEndDate))
            {
                hasBreaksInLearningChanged = true;

                _entity.BreaksInLearning.Add(new EnglishAndMathsBreakInLearning
                {
                    StartDate = newBreakInLearning.StartDate,
                    EndDate = newBreakInLearning.EndDate,
                    PriorPeriodExpectedEndDate = newBreakInLearning.PriorPeriodExpectedEndDate,
                    EnglishAndMathsKey = Key,
                    Key = Guid.NewGuid()
                });
            }
        }

        return hasBreaksInLearningChanged || removedItems.Count > 0;
    }

    internal EnglishAndMaths GetEntity()
    {
        return _entity;
    }

    /// <summary>
    /// Updates general details of the English and Maths course. Returns true if there were changes, otherwise false.
    /// </summary>
    /// <returns>True if the course was updated, otherwise false.</returns>
    internal bool Update(EnglishAndMathsUpdateDetails incomingCourse)
    {
        var hasChanged =
            _entity.Course.Trim() != incomingCourse.Course.Trim() ||
            _entity.StartDate != incomingCourse.StartDate ||
            _entity.PlannedEndDate != incomingCourse.PlannedEndDate ||
            _entity.CompletionDate != incomingCourse.CompletionDate ||
            _entity.PauseDate != incomingCourse.PauseDate ||
            _entity.Amount != incomingCourse.Amount ||
            _entity.CombinedFundingAdjustmentPercentage != incomingCourse.CombinedFundingAdjustmentPercentage;

        if (!hasChanged)
            return false;

        _entity.Course = incomingCourse.Course;
        _entity.StartDate = incomingCourse.StartDate;
        _entity.PlannedEndDate = incomingCourse.PlannedEndDate;
        _entity.CompletionDate = incomingCourse.CompletionDate;
        _entity.PauseDate = incomingCourse.PauseDate;
        _entity.Amount = incomingCourse.Amount;
        _entity.CombinedFundingAdjustmentPercentage = incomingCourse.CombinedFundingAdjustmentPercentage;

        return true;
    }

    /// <summary>
    /// Updates the withdrawal date if it has changed. Returns true if updated, otherwise false.
    /// </summary>
    /// <returns>True if the course was updated, otherwise false.</returns>
    internal bool UpdateWithdrawalDate(EnglishAndMathsUpdateDetails incomingCourse)
    {
        if(_entity.WithdrawalDate == incomingCourse.WithdrawalDate)
            return false;

        _entity.WithdrawalDate = incomingCourse.WithdrawalDate;
        return true;
    }
}
