using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenApplyingEmployerApproval
{
    private ApprenticeshipLearningDomainModel _learning;
    private DataAccess.Entities.Learning.ApprenticeshipEpisode _episode;
    private ApprovedChange[] _changes;

    [SetUp]
    public void SetUp()
    {
        (_learning, _) = new LearningDomainModelBuilder().Build();
        _episode = _learning.GetEntity().Episodes.Single();
        _episode.FurtherApprovalNeeded = true;
        _episode.LearnerRef = "LEARNER-REF";
        _changes = [new ApprovedChange("StartDate", "2025-07-01", "2025-08-01")];
    }

    [Test]
    public void ThenTheFlagIsCleared()
    {
        //Act
        _learning.ApplyEmployerApproval(_episode.Key, _changes);

        //Assert
        _episode.FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void ThenAnEmployerApprovalAppliedEventIsRaised()
    {
        //Act
        _learning.ApplyEmployerApproval(_episode.Key, _changes);

        //Assert
        var raisedEvent = _learning.FlushEvents().OfType<EmployerApprovalAppliedEvent>().Single();
        raisedEvent.LearnerKey.Should().Be(_learning.LearnerKey);
        raisedEvent.LearningKey.Should().Be(_learning.Key);
        raisedEvent.EpisodeKey.Should().Be(_episode.Key);
        raisedEvent.LearnerRef.Should().Be("LEARNER-REF");
        raisedEvent.ApprenticeshipId.Should().Be(_episode.ApprovalsApprenticeshipId);
        raisedEvent.LearningType.Should().Be(_learning.LearningType);
        raisedEvent.Changes.Should().BeEquivalentTo(_changes);
    }

    [Test]
    public void ThenTheFurtherApprovedHistoryEventIsStillRaised()
    {
        //Act
        _learning.ApplyEmployerApproval(_episode.Key, _changes);

        //Assert
        _learning.FlushEvents().OfType<ApprenticeshipLearningChangedEvent>().Single()
            .Operation.Should().Be(ApprenticeshipLearningOperation.FurtherApproved);
    }
}
