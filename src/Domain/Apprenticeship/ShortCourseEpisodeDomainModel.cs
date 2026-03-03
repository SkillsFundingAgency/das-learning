using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseEpisodeDomainModel : EpisodeDomainModel
{
    private readonly DataAccess.Entities.Learning.ShortCourseEpisode _entity;
    private readonly List<ShortCourseMilestoneDomainModel> _milestones;

    public Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public long Ukprn => _entity.Ukprn;
    public long EmployerAccountId => _entity.EmployerAccountId;
    public string TrainingCode => _entity.TrainingCode;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime ExpectedEndDate => _entity.ExpectedEndDate;
    public DateTime StartDate => _entity.StartDate;
    public bool IsApproved => _entity.IsApproved;
    
    public IReadOnlyCollection<ShortCourseMilestoneDomainModel> Milestones =>
        new ReadOnlyCollection<ShortCourseMilestoneDomainModel>(_milestones);

    public IReadOnlyCollection<ShortCourseLearningSupportDomainModel> LearningSupport => _entity.LearningSupport.SelectOrEmptyList(ShortCourseLearningSupportDomainModel.Get);

    internal static ShortCourseEpisodeDomainModel New(
        Guid learningKey,
        long ukprn,
        long employerAccountId,
        string trainingCode,
        bool isApproved,
        DateTime startDate,
        DateTime expectedEndDate,
        DateTime? withdrawalDate)
    {
        return new ShortCourseEpisodeDomainModel(new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = learningKey,
            Ukprn = ukprn,
            EmployerAccountId = employerAccountId,
            TrainingCode = trainingCode,
            IsApproved = isApproved,
            StartDate = startDate,
            ExpectedEndDate = expectedEndDate,
            WithdrawalDate = withdrawalDate            
        });
    }

    public static ShortCourseEpisodeDomainModel Get(ShortCourseEpisode entity)
        => new(entity);

    public ShortCourseEpisode GetEntity() => _entity;

    public void AddMilestone(Milestone milestone)
    {
        var milestoneDomainModel = ShortCourseMilestoneDomainModel.New(
            milestone);

        _milestones.Add(milestoneDomainModel);
        _entity.Milestones.Add(milestoneDomainModel.GetEntity());
    }

    public void AddLearningSupport(DateTime startDate, DateTime endDate)
    {
        _entity.LearningSupport.Add(new ShortCourseLearningSupport
        {
            StartDate = startDate,
            EndDate = endDate,
            LearningKey = _entity.LearningKey,
            EpisodeKey = _entity.Key,
            Key = Guid.NewGuid()
        });
    }

    public void Approve()
    {
        _entity.IsApproved = true;
    }

    private ShortCourseEpisodeDomainModel(ShortCourseEpisode entity)
    {
        _entity = entity;
        _milestones = entity.Milestones
            .Select(ShortCourseMilestoneDomainModel.Get)
            .ToList();
    }
}