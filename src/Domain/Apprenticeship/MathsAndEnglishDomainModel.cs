using SFA.DAS.Learning.DataAccess.Entities.Learning;

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

    internal MathsAndEnglishDomainModel(MathsAndEnglish entity)
    {
        _entity = entity;
    }

    public static MathsAndEnglishDomainModel Get(MathsAndEnglish entity)
    {
        return new MathsAndEnglishDomainModel(entity);
    }
}