using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Domain.Models;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class MathsAndEnglishDomainModel
{
    private readonly MathsAndEnglish _entity;

    public Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime PlannedEndDate => _entity.PlannedEndDate;
    public string Course => _entity.Course;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public DateTime? PauseDate => _entity.PauseDate;
    public int? PriorLearningPercentage => _entity.PriorLearningPercentage;
    public decimal Amount => _entity.Amount;

    internal MathsAndEnglishDomainModel(MathsAndEnglish entity)
    {
        _entity = entity;
    }

    public static MathsAndEnglishDomainModel Get(MathsAndEnglish entity)
    {
        return new MathsAndEnglishDomainModel(entity);
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

                _entity.BreaksInLearning.Add(new MathsAndEnglishBreakInLearning
                {
                    StartDate = newBreakInLearning.StartDate,
                    EndDate = newBreakInLearning.EndDate,
                    PriorPeriodExpectedEndDate = newBreakInLearning.PriorPeriodExpectedEndDate,
                    MathsAndEnglishKey = Key,
                    Key = Guid.NewGuid()
                });
            }
        }

        return hasBreaksInLearningChanged || removedItems.Count > 0;
    }
}