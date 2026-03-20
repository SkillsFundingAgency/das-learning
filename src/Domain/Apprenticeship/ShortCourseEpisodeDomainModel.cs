using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Extensions;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseEpisodeDomainModel : EpisodeDomainModel
{
    private readonly DataAccess.Entities.Learning.ShortCourseEpisode _entity;
    private readonly List<ShortCourseMilestoneDomainModel> _milestones;

    public override Guid Key => _entity.Key;
    public Guid LearningKey => _entity.LearningKey;
    public long Ukprn => _entity.Ukprn;
    public long EmployerAccountId => _entity.EmployerAccountId;
    public string TrainingCode => _entity.TrainingCode;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime ExpectedEndDate => _entity.ExpectedEndDate;
    public DateTime StartDate => _entity.StartDate;
    public bool IsApproved => _entity.IsApproved;
    public decimal Price => _entity.Price;
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
        DateTime? withdrawalDate,
        decimal price = 0)
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
            WithdrawalDate = withdrawalDate,
            Price = price
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

    public void Approve(long employerAccountId)
    {
        _entity.IsApproved = true;
        _entity.EmployerAccountId = employerAccountId;
    }

    public void Update(ShortCourseUpdateContext updateContext)
    {
        _entity.Ukprn = updateContext.OnProgramme.Ukprn;
        _entity.EmployerAccountId = updateContext.OnProgramme.EmployerId;
        _entity.TrainingCode = updateContext.OnProgramme.CourseCode;
        _entity.StartDate = updateContext.OnProgramme.StartDate;
        _entity.ExpectedEndDate = updateContext.OnProgramme.ExpectedEndDate;
        _entity.WithdrawalDate = updateContext.OnProgramme.WithdrawalDate;
        _entity.Price = updateContext.OnProgramme.Price;

        UpdateMilestones(updateContext.OnProgramme.Milestones);
        UpdateLearningSupport(updateContext.LearningSupport);
    }

    private void UpdateMilestones(IEnumerable<Milestone> milestones)
    {
        _entity.Milestones.RemoveWhere(x=> !milestones.Any(m => m == x.Milestone));

        var missingMilestones = milestones.Where(m => !_entity.Milestones.Any(em => em.Milestone == m));

        foreach (var milestone in missingMilestones)
        {
            AddMilestone(milestone);
        }

        _milestones.Clear();
        _milestones.AddRange(_entity.Milestones.Select(ShortCourseMilestoneDomainModel.Get));
    }

    private void UpdateLearningSupport(List<LearningSupportDetails> updatedlearningSupport)
    {
        _entity.LearningSupport.RemoveWhere(x => !updatedlearningSupport.Any(ls => ls.StartDate == x.StartDate && ls.EndDate == x.EndDate));

        var missingLearningSupport = updatedlearningSupport.Where(ls => !_entity.LearningSupport.Any(els => els.StartDate == ls.StartDate && els.EndDate == ls.EndDate));

        foreach (var learningSupport in missingLearningSupport)
        {
            AddLearningSupport(learningSupport.StartDate, learningSupport.EndDate);
        }
    }

    private ShortCourseEpisodeDomainModel(ShortCourseEpisode entity)
    {
        _entity = entity;
        _milestones = entity.Milestones
            .Select(ShortCourseMilestoneDomainModel.Get)
            .ToList();
    }
}