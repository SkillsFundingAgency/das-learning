using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class EpisodeBreakInLearningDomainModel
{
    private readonly EpisodeBreakInLearning _entity;

    public Guid Key => _entity.Key;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public DateTime StartDate => _entity.StartDate;
    public DateTime EndDate => _entity.EndDate;
    public DateTime PriorPeriodExpectedEndDate => _entity.PriorPeriodExpectedEndDate;

    internal static EpisodeBreakInLearningDomainModel New(
        Guid episodeKey,
        DateTime startDate,
        DateTime endDate)
    {
        return new EpisodeBreakInLearningDomainModel(new EpisodeBreakInLearning
        {
            Key = Guid.NewGuid(),
            EpisodeKey = episodeKey,
            StartDate = startDate,
            EndDate = endDate
        });
    }

    public EpisodeBreakInLearning GetEntity()
    {
        return _entity;
    }

    public static EpisodeBreakInLearningDomainModel Get(EpisodeBreakInLearning entity)
    {
        return new EpisodeBreakInLearningDomainModel(entity);
    }

    public void UpdateDates(DateTime startDate, DateTime endDate)
    {
        _entity.StartDate = startDate;
        _entity.EndDate = endDate;
    }

    private EpisodeBreakInLearningDomainModel(EpisodeBreakInLearning entity)
    {
        _entity = entity;
    }
}