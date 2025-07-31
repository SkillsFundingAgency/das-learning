using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class LearningSupportDomainModel
{
    private readonly LearningSupport _entity;
    public Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;

    internal LearningSupportDomainModel(LearningSupport entity)
    {
        _entity = entity;
    }

    public static LearningSupportDomainModel Get(LearningSupport entity)
    {
        return new LearningSupportDomainModel(entity);
    }
}