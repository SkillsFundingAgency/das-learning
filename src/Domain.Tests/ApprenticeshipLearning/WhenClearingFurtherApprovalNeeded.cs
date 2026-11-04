using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Events;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using System;
using System.Linq;

namespace SFA.DAS.Learning.Domain.UnitTests.ApprenticeshipLearning;

[TestFixture]
public class WhenClearingFurtherApprovalNeeded
{
    private ApprenticeshipLearningDomainModel _learning;

    [SetUp]
    public void SetUp()
    {
        (_learning, _) = new LearningDomainModelBuilder().Build();
    }

    [Test]
    public void AndFlagIsSetThenItIsCleared()
    {
        //Arrange
        var episode = _learning.GetEntity().Episodes.Single();
        episode.FurtherApprovalNeeded = true;

        //Act
        ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(episode.Key);

        //Assert
        episode.FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndFlagIsAlreadyClearThenItStaysClear()
    {
        //Arrange
        var episode = _learning.GetEntity().Episodes.Single();
        episode.FurtherApprovalNeeded = false;

        //Act
        ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(episode.Key);

        //Assert
        episode.FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndFlagIsClearedThenTrueIsReturned()
    {
        //Act
        var result = ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(_learning.GetEntity().Episodes.Single().Key);

        //Assert
        result.Should().BeTrue();
    }

    [Test]
    public void AndEpisodeKeyIsNotFoundThenFalseIsReturned()
    {
        //Act
        var result = ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(Guid.NewGuid());

        //Assert
        result.Should().BeFalse();
    }

    [Test]
    public void AndFlagIsSetThenAFurtherApprovedEventIsRaised()
    {
        //Arrange
        var episode = _learning.GetEntity().Episodes.Single();
        episode.FurtherApprovalNeeded = true;

        //Act
        ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(episode.Key);

        //Assert
        var raisedEvent = _learning.FlushEvents().OfType<ApprenticeshipLearningChangedEvent>().Single();
        raisedEvent.LearningKey.Should().Be(_learning.Key);
        raisedEvent.AcademicYear.Should().BeNull();
        raisedEvent.Operation.Should().Be(ApprenticeshipLearningOperation.FurtherApproved);
        raisedEvent.Changes.Should().BeEmpty();
        raisedEvent.Snapshot.Episodes.Single().FurtherApprovalNeeded.Should().BeFalse();
    }

    [Test]
    public void AndFlagIsAlreadyClearThenNoEventIsRaised()
    {
        //Arrange
        var episode = _learning.GetEntity().Episodes.Single();
        episode.FurtherApprovalNeeded = false;

        //Act
        ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(episode.Key);

        //Assert
        _learning.FlushEvents().Should().BeEmpty();
    }

    [Test]
    public void AndEpisodeKeyIsNotFoundThenNoEventIsRaised()
    {
        //Act
        ((LearningDomainModel)_learning).ClearFurtherApprovalNeeded(Guid.NewGuid());

        //Assert
        _learning.FlushEvents().Should().BeEmpty();
    }
}
