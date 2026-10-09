using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using System;
using System.Collections.Generic;

namespace SFA.DAS.Learning.Domain.UnitTests.ShortCourseLearning;

[TestFixture]
public class WhenClearingFurtherApprovalNeeded
{
    [Test]
    public void ThenNotImplementedIsThrown()
    {
        //Arrange
        var fixture = new Fixture();
        var episode = new DataAccess.Entities.Learning.ShortCourseEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            StartDate = new DateTime(2024, 1, 1),
            ExpectedEndDate = new DateTime(2024, 6, 1),
            Ukprn = fixture.Create<long>(),
            TrainingCode = "CODE",
            LearnerRef = "LEARNER1",
            EmployerType = EmployerType.NonLevy
        };
        var entity = new DataAccess.Entities.Learning.ShortCourseLearning
        {
            Key = episode.LearningKey,
            LearnerKey = Guid.NewGuid(),
            Episodes = new List<DataAccess.Entities.Learning.ShortCourseEpisode> { episode }
        };
        var learning = ShortCourseLearningDomainModel.Get(entity);

        //Act
        var act = () => ((LearningDomainModel)learning).ClearFurtherApprovalNeeded(episode.Key);

        //Assert
        act.Should().Throw<NotImplementedException>();
    }
}
