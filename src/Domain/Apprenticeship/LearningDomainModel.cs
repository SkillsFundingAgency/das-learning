using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;
using System.Collections.ObjectModel;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public abstract class LearningDomainModel : AggregateRoot
{
    public abstract void Approve();
}

public abstract class LearningDomainModel<T> : LearningDomainModel where T : Learning.DataAccess.Entities.Learning.Learning
{
    protected T _entity;
    public Guid LearnerKey => _entity.LearnerKey;

    protected LearningDomainModel(T entity)
    {
        _entity = entity;
    }
}

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

    public override void Approve()
    {
        LatestEpisode.Approve();
        //todo: AddEvent();
    }

    public ShortCourseEpisodeDomainModel LatestEpisode
    {
        get
        {
            var latestEpisode = _episodes.MaxBy(x => x.StartDate);
            return latestEpisode;
        }
    }


    private ShortCourseLearningDomainModel(ShortCourseLearning entity): base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}