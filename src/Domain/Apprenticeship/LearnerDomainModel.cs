using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.Models.Apprenticeships;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class LearnerDomainModel : AggregateRoot
{
    private readonly Learning.DataAccess.Entities.Learning.Learner _entity;

    public Guid Key => _entity.Key;
    public string Uln => _entity.Uln;
    public string FirstName => _entity.FirstName;
    public string LastName => _entity.LastName;
    public string? EmailAddress => _entity.EmailAddress;
    public bool HasEHCP => _entity.HasEHCP;
    public bool IsCareLeaver => _entity.IsCareLeaver;
    public bool CareLeaverEmployerConsentGiven => _entity.CareLeaverEmployerConsentGiven;
    public DateTime DateOfBirth => _entity.DateOfBirth;

    internal static LearnerDomainModel New(
        string uln,
        DateTime dateOfBirth,
        string firstName,
        string lastName)
    {
        return new LearnerDomainModel(new Learning.DataAccess.Entities.Learning.Learner
        {
            Key = Guid.NewGuid(),
            Uln = uln,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        });
    }

    public static LearnerDomainModel Get(Learning.DataAccess.Entities.Learning.Learner entity)
    {
        return new LearnerDomainModel(entity);
    }

    private LearnerDomainModel(Learning.DataAccess.Entities.Learning.Learner entity)
    {
        _entity = entity;
    }

    public Learning.DataAccess.Entities.Learning.Learner GetEntity()
    {
        return _entity;
    }

    public LearningUpdateChanges[] Update(LearningUpdateContext updateContext)
    {
        var changes = new List<LearningUpdateChanges>();

        UpdatePersonalDetails(updateContext, changes);

        UpdateLearnerDateOfBirth(updateContext, changes);

        UpdateCareDetails(updateContext, changes);

       // if (changes.Any()) AddEvent(this.ToLearnerUpdatedEvent(updateContext.Learner));

        return changes.ToArray();
    }

    private void UpdatePersonalDetails(LearningUpdateContext updateContext, List<LearningUpdateChanges> changes)
    {
        if (updateContext.Learner.FirstName != FirstName || updateContext.Learner.LastName != LastName ||
            updateContext.Learner.EmailAddress != EmailAddress)
        {
            _entity.FirstName = updateContext.Learner.FirstName;
            _entity.LastName = updateContext.Learner.LastName;
            _entity.EmailAddress = updateContext.Learner.EmailAddress;

            changes.Add(LearningUpdateChanges.PersonalDetails);

            var @event = new PersonalDetailsChangedEvent
            {
                ApprovalsApprenticeshipId = updateContext.ApprovalsApprenticeshipId,
                LearningKey = updateContext.LearningKey,
                FirstName = FirstName,
                LastName = LastName,
                EmailAddress = EmailAddress
            };

            AddEvent(@event);
        }
    }

    private void UpdateLearnerDateOfBirth(LearningUpdateContext updateContext, List<LearningUpdateChanges> changes)
    {
        if (updateContext.Learner.DateOfBirth != DateOfBirth)
        {
            _entity.DateOfBirth = updateContext.Learner.DateOfBirth;

            changes.Add(LearningUpdateChanges.DateOfBirthChanged);

            var @event = new DateOfBirthChangedEvent
            {
                LearningKey = Key,
                DateOfBirth = DateOfBirth
            };

            AddEvent(@event);
        }
    }

    private void UpdateCareDetails(LearningUpdateContext updateContext, List<LearningUpdateChanges> changes)
    {
        if (_entity.HasEHCP != updateContext.Care.HasEHCP ||
            _entity.IsCareLeaver != updateContext.Care.IsCareLeaver ||
            _entity.CareLeaverEmployerConsentGiven != updateContext.Care.CareLeaverEmployerConsentGiven)
        {
            _entity.HasEHCP = updateContext.Care.HasEHCP;
            _entity.IsCareLeaver = updateContext.Care.IsCareLeaver;
            _entity.CareLeaverEmployerConsentGiven = updateContext.Care.CareLeaverEmployerConsentGiven;
            changes.Add(LearningUpdateChanges.Care);
        }
    }
}