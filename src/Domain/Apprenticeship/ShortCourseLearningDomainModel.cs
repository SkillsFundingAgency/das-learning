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
        IEnumerable<Milestone> milestones,
        decimal price = 0)
    {
        var episode = ShortCourseEpisodeDomainModel.New(
            _entity.Key,
            ukprn,
            employerAccountId,
            trainingCode,
            isApproved,
            startDate,
            expectedEndDate,
            withdrawalDate,
            price
        );

        foreach (var milestone in milestones)
        {
            episode.AddMilestone(milestone);
        }

        _episodes.Add(episode);
        _entity.Episodes.Add(episode.GetEntity());

        return episode;
    }

    public ShortCourseUpdateChanges[] Update(ShortCourseUpdateContext updateContext)
    {
        var changes = new List<ShortCourseUpdateChanges>();
        UpdateCompletionDate(updateContext.OnProgramme.CompletionDate, changes);
        UpdateEpisode(updateContext, changes);
        return changes.ToArray();
    }

    private void UpdateCompletionDate(DateTime? completionDate, List<ShortCourseUpdateChanges> changes)
    {
        if (_entity.CompletionDate != completionDate)
            changes.Add(ShortCourseUpdateChanges.CompletionDate);
        _entity.CompletionDate = completionDate;
    }

    private void UpdateEpisode(ShortCourseUpdateContext updateContext, List<ShortCourseUpdateChanges> changes)
    {
        var episode = _episodes.Single();

        var prevWithdrawalDate = episode.WithdrawalDate;
        var prevMilestones = episode.Milestones.Select(m => m.Milestone).ToHashSet();

        episode.Update(updateContext);

        if (episode.WithdrawalDate != prevWithdrawalDate)
            changes.Add(ShortCourseUpdateChanges.WithdrawalDate);

        if (!episode.Milestones.Select(m => m.Milestone).ToHashSet().SetEquals(prevMilestones))
            changes.Add(ShortCourseUpdateChanges.Milestone);
    }

    private ShortCourseLearningDomainModel(ShortCourseLearning entity) : base(entity)
    {
        _entity = entity;
        _episodes = entity.Episodes
            .Select(ShortCourseEpisodeDomainModel.Get)
            .ToList();
    }
}
