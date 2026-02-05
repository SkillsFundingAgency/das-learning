using System.Collections.ObjectModel;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

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

    public IReadOnlyCollection<ShortCourseMilestoneDomainModel> Milestones =>
        new ReadOnlyCollection<ShortCourseMilestoneDomainModel>(_milestones);

    internal static ShortCourseEpisodeDomainModel New(
        long ukprn,
        long employerAccountId,
        string trainingCode)
    {
        return new ShortCourseEpisodeDomainModel(new ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            Ukprn = ukprn,
            EmployerAccountId = employerAccountId,
            TrainingCode = trainingCode
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


    private ShortCourseEpisodeDomainModel(ShortCourseEpisode entity)
    {
        _entity = entity;
        _milestones = entity.Milestones
            .Select(ShortCourseMilestoneDomainModel.Get)
            .ToList();
    }
}