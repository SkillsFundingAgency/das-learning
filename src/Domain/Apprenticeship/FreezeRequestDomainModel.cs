using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class FreezeRequestDomainModel
{
    private readonly FreezeRequest _entity;

    public Guid Key => _entity.Key;
    public Guid ApprenticeshipKey => _entity.LearningKey;
    public string FrozenBy => _entity.FrozenBy;
    public DateTime FrozenDateTime => _entity.FrozenDateTime;
    public bool Unfrozen => _entity.Unfrozen;
    public DateTime? UnfrozenDateTime => _entity.UnfrozenDateTime;
    public string? UnfrozenBy => _entity.UnfrozenBy;
    public string? Reason => _entity.Reason;

    internal static FreezeRequestDomainModel New(Guid apprenticeshipKey, string userId, DateTime frozenDateTime, string? reason)
    {
        return new FreezeRequestDomainModel(new FreezeRequest
        {
            Key = Guid.NewGuid(),
            LearningKey = apprenticeshipKey,
            FrozenBy = userId,
            FrozenDateTime = frozenDateTime,
            Unfrozen = false,
            Reason = reason
        });
    }

    public FreezeRequest GetEntity()
    {
        return _entity;
    }

    internal static FreezeRequestDomainModel Get(FreezeRequest entity)
    {
        return new FreezeRequestDomainModel(entity);
    }

    internal void Unfreeze(string userId, DateTime changeDateTime)
    {
        _entity.Unfrozen = true;
        _entity.UnfrozenBy = userId;
        _entity.UnfrozenDateTime = changeDateTime;
    }

    private FreezeRequestDomainModel(FreezeRequest entity)
    {
        _entity = entity;
    }
}
