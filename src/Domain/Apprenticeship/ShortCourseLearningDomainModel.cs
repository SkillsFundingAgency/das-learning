using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class ShortCourseLearningDomainModel : LearningDomainModel<Learning.DataAccess.Entities.Learning.ShortCourseLearning>
{
    private readonly List<ShortCourseEpisodeDomainModel> _episodes;

    public Guid Key => _entity.Key;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public IReadOnlyCollection<ShortCourseEpisodeDomainModel> Episodes => new ReadOnlyCollection<ShortCourseEpisodeDomainModel>(_episodes);

    internal static ShortCourseLearningDomainModel New(
        Guid learnerKey,
        DateTime? completionDate)
    {
        return new ShortCourseLearningDomainModel(new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = learnerKey,
            CompletionDate = completionDate
        });
    }

    public static ShortCourseLearningDomainModel Get(ShortCourseLearning entity)
    {
        return new ShortCourseLearningDomainModel(entity);
    }

    public Learning.DataAccess.Entities.Learning.ShortCourseLearning GetEntity()
    {
        return _entity;
    }

    public ShortCourseEpisodeDomainModel AddEpisode(
        long ukprn,
        long employerAccountId,
        string trainingCode,
        bool isApproved,
        DateTime startDate,
        DateTime expectedEndDate,
        DateTime? withdrawalDate,
        IEnumerable<Milestone> milestones)
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            _entity.Key,
            ukprn,
            employerAccountId,
            trainingCode,
            isApproved,
            startDate,
            expectedEndDate,
            withdrawalDate
        );

        foreach (var milestone in milestones)
        {
            episode.AddMilestone(milestone);
        }

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());

        return episode;
    }

    public void Update(ShortCourseUpdateContext updateContext)
    {
        _entity.CompletionDate = updateContext.OnProgramme.CompletionDate;
        var episode = _episodes.Single();
        episode.Update(updateContext);
    }

    private ShortCourseLearningDomainModel(ShortCourseLearning entity) : base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}
