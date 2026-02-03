using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class MathsAndEnglishBreakInLearningDomainModel
{
    private readonly MathsAndEnglishBreakInLearning _entity;

    public Guid Key => _entity.Key;
    public Guid MathsAndEnglishKey => _entity.MathsAndEnglishKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;

    internal static MathsAndEnglishBreakInLearningDomainModel New(
        Guid mathsAndEnglishKey,
        DateTime startDate,
        DateTime endDate)
    {
        return new MathsAndEnglishBreakInLearningDomainModel(new MathsAndEnglishBreakInLearning
        {
            Key = Guid.NewGuid(),
            MathsAndEnglishKey = mathsAndEnglishKey,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public MathsAndEnglishBreakInLearning GetEntity()
    {
        return _entity;
    }

    public static MathsAndEnglishBreakInLearningDomainModel Get(MathsAndEnglishBreakInLearning entity)
    {
        return new MathsAndEnglishBreakInLearningDomainModel(entity);
    }

    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        _entity.StartDate = startDate;
        _entity.EndDate = endDate;
    }

    private MathsAndEnglishBreakInLearningDomainModel(MathsAndEnglishBreakInLearning entity)
    {
        _entity = entity;
    }
}