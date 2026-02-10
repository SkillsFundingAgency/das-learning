using SFA.DAS.Learning.Domain.Models.Apprenticeships;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

public class LearnerDomainModel : LearningDomainModel
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

    /// <summary>
    /// Returns true if personal details were updated
    /// </summary>
    public bool UpdatePersonalDetails(LearningUpdateDetails updateModel)
    {
        if (updateModel.FirstName != FirstName || 
            updateModel.LastName != LastName ||
            updateModel.EmailAddress != EmailAddress)
        {
            _entity.FirstName = updateModel.FirstName;
            _entity.LastName = updateModel.LastName;
            _entity.EmailAddress = updateModel.EmailAddress;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if date of birth was updated
    /// </summary>
    public bool UpdateLearnerDateOfBirth(LearnerUpdateModel updateModel)
    {
        if (updateModel.Learning.DateOfBirth != DateOfBirth)
        {
            _entity.DateOfBirth = updateModel.Learning.DateOfBirth;

            return true;
        }

        return false;
    }

    public bool UpdateCareDetails(LearnerUpdateModel updateModel)
    {
        if (_entity.HasEHCP != updateModel.Learning.Care.HasEHCP ||
            _entity.IsCareLeaver != updateModel.Learning.Care.IsCareLeaver ||
            _entity.CareLeaverEmployerConsentGiven != updateModel.Learning.Care.CareLeaverEmployerConsentGiven)
        {
            _entity.HasEHCP = updateModel.Learning.Care.HasEHCP;
            _entity.IsCareLeaver = updateModel.Learning.Care.IsCareLeaver;
            _entity.CareLeaverEmployerConsentGiven = updateModel.Learning.Care.CareLeaverEmployerConsentGiven;
            
            return true;
        }
        return false;
    }
}