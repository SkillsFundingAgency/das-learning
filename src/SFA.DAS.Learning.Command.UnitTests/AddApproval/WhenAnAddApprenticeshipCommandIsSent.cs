using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using SFA.DAS.Learning.Types;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Learning.Enums;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.UnitTests.AddApproval;

[TestFixture]
public class WhenAnAddApprenticeshipCommandIsSent
{
    private AddLearningCommandHandler _commandHandler = null!;
    private Mock<ILearningService> _learningService = null!;
    private Mock<ILearnerFactory> _learnerFactory = null!;
    private Mock<IApprenticeshipLearningFactory> _apprenticeshipFactory = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IMessageSession> _messageSession = null!;
    private Mock<ILogger<AddLearningCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learningService = new Mock<ILearningService>();
        _learnerFactory = new Mock<ILearnerFactory>();
        _apprenticeshipFactory = new Mock<IApprenticeshipLearningFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _messageSession = new Mock<IMessageSession>();
        _logger = new Mock<ILogger<AddLearningCommandHandler>>();
        _commandHandler = new AddLearningCommandHandler(
            _learningService.Object,
            _learnerFactory.Object,
            _apprenticeshipFactory.Object, 
            _learnerRepository.Object,
            _messageSession.Object,
            _logger.Object);

        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
	public async Task WhenAnApprenticeshipAlreadyExistsThenItIsNotCreatedAgain()
    {
        var command = _fixture.Create<AddLearningCommand>();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();

        _learningService.Setup(x => x.GetLearning(command.Uln,  LearningType.Apprenticeship, false, command.ApprovalsApprenticeshipId))
            .ReturnsAsync(apprenticeship);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.IsAny<ApprenticeshipLearningDomainModel>(), It.IsAny<LearningType>()), Times.Never());
    }
	
    [Test]
    public async Task ThenAnEpisodeIsCreated()
    {
        var command = _fixture.Create<AddLearningCommand>();
        var trainingCodeInt = _fixture.Create<int>();
        command.TrainingCode = trainingCodeInt.ToString();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName, null)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);
        
        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Count == 1), It.IsAny<LearningType>()));
        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Count == 1), It.IsAny<LearningType>()));
    }

    [Test]
    public async Task AndNoActualStartDateSet_ThenEpisodeIsCreatedUsingPlannedStartDate()
    {
        var command = _fixture.Create<AddLearningCommand>();
        var trainingCodeInt = _fixture.Create<int>();
        command.TrainingCode = trainingCodeInt.ToString();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        command.ActualStartDate = null;

        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName, null)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Single().StartDate == command.PlannedStartDate), It.IsAny<LearningType>()));
    }

    [Test]
    public async Task ThenEventPublished()
    {
        // Arrange
        var command = _fixture.Create<AddLearningCommand>();
        command.FundingPlatform = FundingPlatform.DAS;
        var trainingCodeInt = _fixture.Create<int>();
        command.TrainingCode = trainingCodeInt.ToString();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();

        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName, null)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(It.Is<LearningCreatedEvent>(e =>
            DoApprenticeshipDetailsMatchDomainModel(e, apprenticeship, learner)
            && ApprenticeshipDomainModelTestHelper.DoEpisodeDetailsMatchDomainModel(e, apprenticeship, learner)), It.IsAny<PublishOptions>(),
            It.IsAny<CancellationToken>()));
    }

    [Test]
    public async Task AndNotFundedByDASThenEventIsNotPublished()
    {
        // Arrange
        var command = _fixture.Create<AddLearningCommand>();
        command.FundingPlatform = FundingPlatform.SLD;
        var trainingCodeInt = _fixture.Create<int>();
        command.TrainingCode = trainingCodeInt.ToString();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName, null)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(It.IsAny<LearningCreatedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }


    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenItIsApproved()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetLearning(command.Uln, LearningType.ApprenticeshipUnit, false, It.IsAny<long>()))
            .ReturnsAsync(shortCourseLearning);

        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisode.IsApproved.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(shortCourseLearning, LearningType.ApprenticeshipUnit));
        _messageSession.Verify(x => x.Publish(It.Is<LearningApprovedEvent>(e => e.LearningKey == shortCourseLearning.Key), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseDoesNotExistThenDoNothing()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        _learningService.Setup(x => x.GetLearning(command.Uln, LearningType.ApprenticeshipUnit, false, It.IsAny<long>()))
            .ReturnsAsync(() => null);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>(), It.IsAny<LearningType>()), Times.Never);
    }

    private static bool DoApprenticeshipDetailsMatchDomainModel(
        LearningCreatedEvent e, ApprenticeshipLearningDomainModel learning, LearnerDomainModel learner)
    {
        return
            e.LearningKey == learning.Key &&
            e.ApprovalsApprenticeshipId == learning.ApprovalsApprenticeshipId &&
            e.Uln == learner.Uln &&
            e.FirstName == learner.FirstName &&
            e.LastName == learner.LastName &&
            e.DateOfBirth == learner.DateOfBirth;
    }
}