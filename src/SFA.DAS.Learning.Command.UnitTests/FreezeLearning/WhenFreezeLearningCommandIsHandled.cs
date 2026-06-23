using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.FreezeLearning;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.FreezeLearning;

[TestFixture]
public class WhenFreezeLearningCommandIsHandled
{
    private Mock<IShortCourseLearningRepository> _mockShortCourseRepository = null!;
    private Mock<ILearnerRepository> _mockLearnerRepository = null!;
    private Mock<ILogger<FreezeLearningCommandHandler>> _mockLogger = null!;
    private Mock<IMessageSession> _mockMessageSession = null!;
    private FreezeLearningCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _mockShortCourseRepository = new Mock<IShortCourseLearningRepository>();
        _mockLearnerRepository = new Mock<ILearnerRepository>();
        _mockMessageSession = new Mock<IMessageSession>();
        _mockLogger = new Mock<ILogger<FreezeLearningCommandHandler>>();

        _handler = new FreezeLearningCommandHandler(
            _mockLogger.Object,
            _mockShortCourseRepository.Object,
            _mockLearnerRepository.Object,
            _mockMessageSession.Object);
    }

    [Test]
    public async Task ThenMatchingShortCourseEpisodePaymentsAreFrozen()
    {
        //  Arrange
        var approvalsApprenticeshipId = 12345L;
        var learning = CreateShortCourseLearning(approvalsApprenticeshipId, isFrozen: false);
        var learner = CreateLearner(learning.GetEntity().LearnerKey);

        _mockShortCourseRepository
            .Setup(x => x.GetByApprovalsApprenticeshipId(approvalsApprenticeshipId))
            .ReturnsAsync(learning);
        _mockLearnerRepository
            .Setup(x => x.Get(learning.GetEntity().LearnerKey))
            .ReturnsAsync(learner);

        //  Act
        await _handler.Handle(new FreezeLearningCommand(approvalsApprenticeshipId));

        //  Assert
        learning.Episodes.Single().PaymentsFrozen.Should().BeTrue();
        _mockShortCourseRepository.Verify(x => x.Update(learning), Times.Once);

        _mockMessageSession.Verify(x => x.Publish(It.Is<PaymentsStatusUpdatedForEpisode>(e =>
            e.PaymentsFrozen == true &&
            e.LearnerKey == learner.Key &&
            e.LearningKey == learning.Key &&
            e.EpisodeKey == learning.Episodes.Single().Key
        ), It.IsAny<PublishOptions>()), Times.Once);

    }

    [Test]
    public async Task ThenNothingHappensWhenNoShortCourseLearningIsFound()
    {
        //  Arrange
        var approvalsApprenticeshipId = 98765L;

        _mockShortCourseRepository
            .Setup(x => x.GetByApprovalsApprenticeshipId(approvalsApprenticeshipId))
            .ReturnsAsync((ShortCourseLearningDomainModel?)null);

        //  Act
        await _handler.Handle(new FreezeLearningCommand(approvalsApprenticeshipId));

        //  Assert
        _mockShortCourseRepository.Verify(x => x.Update(It.IsAny<ShortCourseLearningDomainModel>()), Times.Never);
        _mockLearnerRepository.Verify(x => x.Get(It.IsAny<Guid>()), Times.Never);
    }

    private static ShortCourseLearningDomainModel CreateShortCourseLearning(long approvalsApprenticeshipId, bool isFrozen)
    {
        var learningKey = Guid.NewGuid();
        var learnerKey = Guid.NewGuid();
        var episodeKey = Guid.NewGuid();

        var entity = new ShortCourseLearning
        {
            Key = learningKey,
            LearnerKey = learnerKey,
            TrainingCode = "SC-100",
            Episodes = new List<ShortCourseEpisode>
            {
                new()
                {
                    Key = episodeKey,
                    LearningKey = learningKey,
                    Ukprn = 10005001,
                    EmployerAccountId = 123456,
                    TrainingCode = "SC-100",
                    ApprovalsApprenticeshipId = approvalsApprenticeshipId,
                    IsApproved = true,
                    StartDate = DateTime.UtcNow.Date.AddMonths(-2),
                    ExpectedEndDate = DateTime.UtcNow.Date.AddMonths(4),
                    Price = 1000,
                    LearnerRef = "LR-100",
                    LearningType = LearningType.ApprenticeshipUnit,
                    EmployerType = EmployerType.NonLevy,
                    PaymentsFrozen = isFrozen,
                    Milestones = new List<ShortCourseMilestone>(),
                    LearningSupport = new List<ShortCourseLearningSupport>()
                }
            }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }

    private static LearnerDomainModel CreateLearner(Guid learnerKey)
    {
        return LearnerDomainModel.Get(new Learner
        {
            Key = learnerKey,
            Uln = "1234567890",
            FirstName = "Alex",
            LastName = "Learner",
            DateOfBirth = new DateTime(2000, 1, 1)
        });
    }
}
