using System.Collections.ObjectModel;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public abstract class LearningDomainModel : AggregateRoot
{
}

public class ShortCourseLearningDomainModel : LearningDomainModel
{
    private readonly Learning.DataAccess.Entities.Learning.ShortCourseLearning _entity;
    private readonly List<ShortCourseEpisodeDomainModel> _episodes;

    public Guid Key => _entity.Key;
    public Guid LearnerKey => _entity.LearnerKey;
    public DateTime? WithdrawalDate => _entity.WithdrawalDate;
    public DateTime ExpectedEndDate => _entity.ExpectedEndDate;
    public bool IsApproved => _entity.IsApproved;
    public DateTime? CompletionDate => _entity.CompletionDate;
    public IReadOnlyCollection<ShortCourseEpisodeDomainModel> Episodes => new ReadOnlyCollection<ShortCourseEpisodeDomainModel>(_episodes);

    internal static ShortCourseLearningDomainModel New(
        Guid learnerKey,
        DateTime? withdrawalDate,
        DateTime expectedEndDate,
        DateTime? completionDate,
        bool isApproved)
    {
        return new ShortCourseLearningDomainModel(new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = learnerKey,
            ExpectedEndDate = expectedEndDate,
            CompletionDate = completionDate,
            WithdrawalDate = withdrawalDate,
            IsApproved = isApproved
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

    public void AddEpisode(
        long ukprn,
        long employerAccountId,
        string trainingCode,
        IEnumerable<Milestone> milestones)
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            ukprn,
            employerAccountId,
            trainingCode
        );

        foreach (var milestone in milestones)
        {
            episode.AddMilestone(milestone);
        }

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());
    }

    private ShortCourseLearningDomainModel(ShortCourseLearning entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}