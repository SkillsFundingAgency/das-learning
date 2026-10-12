namespace SFA.DAS.Learning.Types;

/// <summary>
/// Valid values for <see cref="ApprenticeshipFieldChange.ChangeType"/> on an <see cref="ApprovedLearningUpdatedEvent"/>.
/// </summary>
public enum ApprovedLearningUpdatedChangeType : byte
{
    Firstname = 0,
    Surname = 1,
    Email = 2,
    DOB = 3,
    StartDate = 4,
    PlannedEndDate = 5
}
