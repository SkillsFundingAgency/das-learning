using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.AddLearning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.AddApproval;

[TestFixture]
public class WhenAnAddApprenticeshipCommandIsSent
{
    private AddLearningCommandHandler _commandHandler = null!;
    private Mock<ILearningService> _learningService = null!;
    private Mock<ILearnerFactory> _learnerFactory = null!;
    private Mock<IApprenticeshipLearningFactory> _apprenticeshipFactory = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<ILogger<AddLearningCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learningService = new Mock<ILearningService>();
        _learnerFactory = new Mock<ILearnerFactory>();
        _apprenticeshipFactory = new Mock<IApprenticeshipLearningFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _logger = new Mock<ILogger<AddLearningCommandHandler>>();
        _commandHandler = new AddLearningCommandHandler(
            _learningService.Object,
            _learnerFactory.Object,
            _apprenticeshipFactory.Object, 
            _learnerRepository.Object,
            _logger.Object);

        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
	public async Task WhenAnUnapprovedApprenticeshipAlreadyExistsThenItIsApprovedNotCreatedAgain()
    {
        var command = _fixture.Create<AddLearningCommand>();
        var apprenticeship = _fixture.Create<ApprenticeshipLearningDomainModel>();
        TestHelper.SetEpisode(apprenticeship, _fixture.CreateEpisodeDomainModel());

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.Apprenticeship, command.ApprovalsApprenticeshipId, It.IsAny<string>()))
            .ReturnsAsync(apprenticeship);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never());
        _learningService.Verify(x => x.UpdateLearning(apprenticeship), Times.Once);

        apprenticeship.LatestEpisode.IsApproved.Should().BeTrue();
        apprenticeship.LatestEpisode.EmployerAccountId.Should().Be(command.EmployerAccountId);
        apprenticeship.LatestEpisode.EmployerType.Should().Be(command.EmployerType);
        apprenticeship.LatestEpisode.FundingEmployerAccountId.Should().Be(command.TransferSenderId);
        apprenticeship.LatestEpisode.LegalEntityName.Should().Be(command.LegalEntityName);
        apprenticeship.LatestEpisode.ApprovalsApprenticeshipId.Should().Be(command.ApprovalsApprenticeshipId);
        apprenticeship.LatestEpisode.AccountLegalEntityId.Should().Be(command.AccountLegalEntityId);
        apprenticeship.LatestEpisode.TrainingCourseVersion.Should().Be(command.TrainingCourseVersion);

        apprenticeship
            .FlushEvents()
            .OfType<Domain.Events.LearningApprovedEvent>()
            .Should()
            .ContainSingle(e => e.LearningKey == apprenticeship.Key
                                && e.EpisodeKey == apprenticeship.LatestEpisode.Key
                                && e.ApprovalsApprenticeshipId == command.ApprovalsApprenticeshipId
                                && e.EmployerAccountId == command.EmployerAccountId
                                && e.FundingAccountId == (command.TransferSenderId ?? command.EmployerAccountId)
                                && e.LearnerKey == apprenticeship.LearnerKey
                                && e.EmployerType == command.EmployerType);
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
        _apprenticeshipFactory.Setup(x => x.CreateNew(learner.Key)).Returns(apprenticeship);
        
        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Count == 1)));
        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Count == 1)));
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
        _apprenticeshipFactory.Setup(x => x.CreateNew(learner.Key)).Returns(apprenticeship);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.AddLearning(It.Is<ApprenticeshipLearningDomainModel>(y => y.GetEntity().Episodes.Single().Prices.Single().StartDate == command.PlannedStartDate)));
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenItIsApproved()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(shortCourseLearning);

        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;
        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).IsApproved.Should().BeTrue();
        _learningService.Verify(x => x.UpdateLearning(shortCourseLearning));

        shortCourseLearning
            .FlushEvents()
            .OfType<Domain.Events.LearningApprovedEvent>()
            .Should()
            .ContainSingle(e => e.LearningKey == shortCourseLearning.Key
                                && e.EpisodeKey == shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).Key
                                && e.ApprovalsApprenticeshipId == command.ApprovalsApprenticeshipId
                                && e.EmployerAccountId == command.EmployerAccountId
                                && e.FundingAccountId == (command.TransferSenderId ?? command.EmployerAccountId)
                                && e.LearnerKey == shortCourseLearning.LearnerKey
                                && e.LearnerRef == shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).LearnerRef
                                && e.EmployerType == command.EmployerType);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenTheEmployerAccountIdIsUpdated()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(shortCourseLearning);

        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;
        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).EmployerAccountId.Should().Be(command.EmployerAccountId);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenTheEmployerTypeIsUpdated()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(shortCourseLearning);

        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;
        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).EmployerType.Should().Be(command.EmployerType);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenTheApprovalsApprenticeshipIdIsStored()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(shortCourseLearning);

        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;
        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).ApprovalsApprenticeshipId.Should().Be(command.ApprovalsApprenticeshipId);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseExistsThenTheTransferSenderIdIsStored()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(shortCourseLearning);

        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;
        await _commandHandler.Handle(command);

        shortCourseLearning.LatestEpisodeForProvider(command.UKPRN).TransferSenderId.Should().Be(command.TransferSenderId);
    }

    [Test]
    public async Task WhenAnUnapprovedShortCourseDoesNotExistThenDoNothing()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        _learningService.Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(() => null);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task WhenApprovingShortCourseThenTrainingCodeIsPassedToGetUnapprovedLearning()
    {
        var command = _fixture.Build<AddLearningCommand>()
            .With(x => x.LearningType, LearningType.ApprenticeshipUnit)
            .Create();

        var shortCourseLearning = _fixture.Create<ShortCourseLearningDomainModel>();
        command.UKPRN = shortCourseLearning.Episodes.First().Ukprn;

        _learningService
            .Setup(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), command.TrainingCode))
            .ReturnsAsync(shortCourseLearning);

        await _commandHandler.Handle(command);

        _learningService.Verify(x => x.GetUnapprovedLearning(command.Uln, LearningType.ApprenticeshipUnit, It.IsAny<long>(), command.TrainingCode), Times.Once);
    }
}
