using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using SFA.DAS.Learning.Types;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FundingPlatform = SFA.DAS.Learning.Enums.FundingPlatform;

namespace SFA.DAS.Learning.Command.UnitTests.AddApproval;

[TestFixture]
public class WhenAnAddApprenticeshipCommandIsSent
{
    private AddLearningCommandHandler _commandHandler = null!;
    private Mock<ILearnerFactory> _learnerFactory = null!;
    private Mock<IApprenticeshipLearningFactory> _apprenticeshipFactory = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IApprenticeshipLearningRepository> _apprenticeshipRepository = null!;
    private Mock<IMessageSession> _messageSession = null!;
    private Mock<ILogger<AddLearningCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learnerFactory = new Mock<ILearnerFactory>();
        _apprenticeshipFactory = new Mock<IApprenticeshipLearningFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _apprenticeshipRepository = new Mock<IApprenticeshipLearningRepository>();
        _messageSession = new Mock<IMessageSession>();
        _logger = new Mock<ILogger<AddLearningCommandHandler>>();
        _commandHandler = new AddLearningCommandHandler(
            _learnerFactory.Object,
            _apprenticeshipFactory.Object, 
            _learnerRepository.Object,
            _apprenticeshipRepository.Object, 
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

		_apprenticeshipRepository.Setup(x => x.Get(command.Uln, command.ApprovalsApprenticeshipId)).ReturnsAsync(apprenticeship);

        await _commandHandler.Handle(command);

        _apprenticeshipRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never());
    }
	
    [Test]
    public async Task ThenAnEpisodeIsCreated()
    {
        var command = _fixture.Create<AddLearningCommand>();
        var trainingCodeInt = _fixture.Create<int>();
        command.TrainingCode = trainingCodeInt.ToString();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        var learner = _fixture.Create<LearnerDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);
        
        await _commandHandler.Handle(command);

        _apprenticeshipRepository.Verify(x => x.Add(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Count == 1)));
        _apprenticeshipRepository.Verify(x => x.Add(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Count == 1)));
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

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);

        await _commandHandler.Handle(command);

        _apprenticeshipRepository.Verify(x => x.Add(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Single().StartDate == command.PlannedStartDate)));
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

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName)).Returns(learner);
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

        _learnerFactory.Setup(x => x.CreateNew(command.Uln, command.DateOfBirth, command.FirstName, command.LastName)).Returns(learner);
        _apprenticeshipFactory.Setup(x => x.CreateNew(command.ApprovalsApprenticeshipId, learner.Key)).Returns(apprenticeship);

        // Act
        await _commandHandler.Handle(command);

        // Assert
        _messageSession.Verify(x => x.Publish(It.IsAny<LearningCreatedEvent>(), It.IsAny<PublishOptions>(), It.IsAny<CancellationToken>()), Times.Never);
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