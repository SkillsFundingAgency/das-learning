using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class EnglishAndMathsBreakInLearningDomainModel
{
    private readonly EnglishAndMathsBreakInLearning _entity;

    public Guid Key => _entity.Key;
    public Guid EnglishAndMathsKey => _entity.EnglishAndMathsKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;

    internal static EnglishAndMathsBreakInLearningDomainModel New(
        Guid englishAndMathsKey,
        DateTime startDate,
        DateTime endDate)
    {
        return new EnglishAndMathsBreakInLearningDomainModel(new EnglishAndMathsBreakInLearning
        {
            Key = Guid.NewGuid(),
            EnglishAndMathsKey = englishAndMathsKey,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public EnglishAndMathsBreakInLearning GetEntity()
    {
        return _entity;
    }

    public static EnglishAndMathsBreakInLearningDomainModel Get(EnglishAndMathsBreakInLearning entity)
    {
        return new EnglishAndMathsBreakInLearningDomainModel(entity);
    }

    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        _entity.StartDate = startDate;
        _entity.EndDate = endDate;
    }

    private EnglishAndMathsBreakInLearningDomainModel(EnglishAndMathsBreakInLearning entity)
    {
        _entity = entity;
    }
}
