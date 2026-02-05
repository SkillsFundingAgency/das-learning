using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseMilestoneDomainModel
{
    private readonly ShortCourseMilestone _entity;

    public Guid Key => _entity.Key;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public Milestone Milestone => _entity.Milestone;

    internal static ShortCourseMilestoneDomainModel New(
        Milestone milestone)
    {
        return new ShortCourseMilestoneDomainModel(new ShortCourseMilestone
        {
            Key = Guid.NewGuid(),
            Milestone = milestone
        });
    }

    public static ShortCourseMilestoneDomainModel Get(ShortCourseMilestone entity)
    {
        return new ShortCourseMilestoneDomainModel(entity);
    }

    public ShortCourseMilestone GetEntity()
    {
        return _entity;
    }

    private ShortCourseMilestoneDomainModel(ShortCourseMilestone entity)
    {
        _entity = entity;
    }
}