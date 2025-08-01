﻿using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class WithdrawalRequestDomainModel
{
    private readonly WithdrawalRequest _entity;

    public Guid Key => _entity.Key;
    public Guid ApprenticeshipKey => _entity.LearningKey;
    public Guid EpisodeKey => _entity.EpisodeKey;
    public string Reason => _entity.Reason;
    public DateTime LastDayOfLearning => _entity.LastDayOfLearning;
    public DateTime CreatedDate => _entity.CreatedDate;
    public string? ProviderApprovedBy => _entity.ProviderApprovedBy;

    internal static WithdrawalRequestDomainModel New(
        Guid apprenticeshipKey, Guid episodeKey, string reason, DateTime lastDayOfLearning, DateTime createdDate, string providerApprovedBy)
    {
        return new WithdrawalRequestDomainModel(new WithdrawalRequest
        {
            LearningKey = apprenticeshipKey,
            EpisodeKey = episodeKey,
            Reason = reason,
            LastDayOfLearning = lastDayOfLearning,
            CreatedDate = createdDate,
            ProviderApprovedBy = providerApprovedBy
        });
    }

    public WithdrawalRequestDomainModel(WithdrawalRequest entity) 
    {
        _entity = entity;
    }

    public WithdrawalRequest GetEntity()
    {
        return _entity;
    }
}