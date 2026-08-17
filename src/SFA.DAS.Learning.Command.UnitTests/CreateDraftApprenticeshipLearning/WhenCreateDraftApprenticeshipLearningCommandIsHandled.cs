using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftApprenticeshipLearning;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Models.UpdateModels;
using SFA.DAS.Learning.Models.UpdateModels.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.CreateDraftApprenticeshipLearning;

[TestFixture]
public class WhenCreateDraftApprenticeshipLearningCommandIsHandled
{
    private const long Ukprn = 12345678;
    private const string TrainingCode = "ST0001";
    private Fixture _fixture = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IApprenticeshipLearningRepository> _learningRepository = null!;
    private Mock<ILogger<CreateDraftApprenticeshipLearningCommandHandler>> _logger = null!;
    private ILearnerFactory _learnerFactory = null!;
    private IApprenticeshipLearningFactory _apprenticeshipLearningFactory = null!;

    private CreateDraftApprenticeshipLearningCommandHandler _handler = null!;

    [SetUp]
    public void Arrange()
    {
        _fixture = new Fixture();
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningRepository = new Mock<IApprenticeshipLearningRepository>();
        _logger = new Mock<ILogger<CreateDraftApprenticeshipLearningCommandHandler>>();
        _learnerFactory = new LearnerFactory();
        _apprenticeshipLearningFactory = new ApprenticeshipLearningFactory();

        _handler = new CreateDraftApprenticeshipLearningCommandHandler(
            _learnerFactory,
            _apprenticeshipLearningFactory,
            _learnerRepository.Object,
            _learningRepository.Object,
            _logger.Object);

        _learningRepository
            .Setup(x => x.GetOtherUnapprovedCourseLearnings(It.IsAny<Guid>(), It.IsAny<long>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());
    }

    [Test]
    public async Task Then_New_Learner_And_Learning_Are_Created_When_Learner_Does_Not_Exist()
    {
        // Arrange
        var command = CreateCommand();
        ApprenticeshipLearningDomainModel? addedLearning = null;

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync((LearnerDomainModel?)null);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(It.IsAny<Guid>(), It.IsAny<long?>(), It.IsAny<string?>()))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        _learningRepository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(l => addedLearning = l)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Add(It.IsAny<LearnerDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        addedLearning.Should().NotBeNull();
        addedLearning!.LatestEpisode.IsApproved.Should().BeFalse();
        addedLearning.LatestEpisode.EmployerAccountId.Should().BeNull();
        addedLearning.LatestEpisode.EmployerType.Should().Be(EmployerType.Levy);
        addedLearning.LatestEpisode.FundingPlatform.Should().Be(FundingPlatform.SLD);
    }

    [Test]
    public async Task Then_New_Learning_Is_Created_When_Learner_Exists_But_No_Learnings_Exist()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        ApprenticeshipLearningDomainModel? addedLearning = null;

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        _learningRepository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(l => addedLearning = l)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Add(It.IsAny<LearnerDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        addedLearning.Should().NotBeNull();
        addedLearning!.LatestEpisode.IsApproved.Should().BeFalse();
    }

    [Test]
    public async Task Then_New_Learning_Is_Created_When_All_Existing_Learnings_Are_Approved()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var approvedLearning = CreateLearning(isApproved: true);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { approvedLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task Then_Existing_Learning_Is_Reinstated_When_A_Single_Removed_And_Approved_Learning_Exists()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var removedLearning = CreateLearning(isApproved: true, isRemoved: true);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { removedLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learningRepository.Verify(x => x.Update(removedLearning), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        removedLearning.LatestEpisode.IsRemoved.Should().BeFalse();
        removedLearning.LatestEpisode.IsApproved.Should().BeTrue();
        result!.Changes.Should().Contain(LearningUpdateChanges.Reinstated);
    }

    [Test]
    public async Task Then_New_Learning_Is_Created_When_Multiple_Removed_And_Approved_Learnings_Exist()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var firstRemovedLearning = CreateLearning(isApproved: true, isRemoved: true);
        var secondRemovedLearning = CreateLearning(isApproved: true, isRemoved: true);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { firstRemovedLearning, secondRemovedLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task Then_Existing_Unapproved_Learning_Is_Updated_When_A_Single_Unapproved_Learning_Exists()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var unapprovedLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { unapprovedLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Update(learner), Times.Once);
        _learningRepository.Verify(x => x.Update(unapprovedLearning), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        unapprovedLearning.LatestEpisode.IsApproved.Should().BeFalse();
    }

    [Test]
    public async Task Then_Result_Reflects_Updated_Learning_When_Unapproved_Learning_Exists()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var unapprovedLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { unapprovedLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.LearningKey.Should().Be(unapprovedLearning.Key);
        result.LearningEpisodeKey.Should().Be(unapprovedLearning.LatestEpisode.Key);
    }

    [Test]
    public async Task Then_PersonalDetailsChangedEvent_Is_Added_When_Personal_Details_Have_Changed()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var unapprovedLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { unapprovedLearning });

        // Act
        await _handler.Handle(command);

        // Assert
        AssertPersonalDetailsEvent(learner, unapprovedLearning.LatestEpisode.ApprovalsApprenticeshipId, unapprovedLearning.Key, command.LearningUpdateContext.Learner.FirstName, command.LearningUpdateContext.Learner.LastName);
    }

    [Test]
    public async Task Then_GetAllByLearnerKey_Is_Called_With_Ukprn_And_TrainingCode_Filters()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        // Act
        await _handler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode), Times.Once);
    }

    [Test]
    public async Task Then_Exception_Is_Thrown_When_Multiple_Unapproved_Learnings_Exist()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var firstUnapprovedLearning = CreateLearning(isApproved: false);
        var secondUnapprovedLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { firstUnapprovedLearning, secondUnapprovedLearning });

        // Act
        Func<Task> act = () => _handler.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task Then_The_Other_Unapproved_Course_Is_Marked_Removed_When_Exactly_One_Exists()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var missingLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        _learningRepository
            .Setup(x => x.GetOtherUnapprovedCourseLearnings(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { missingLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        missingLearning.LatestEpisode.IsRemoved.Should().BeTrue();
        _learningRepository.Verify(x => x.Update(missingLearning), Times.Once);
        result!.RemovedLearningKey.Should().Be(missingLearning.Key);
    }

    [Test]
    public async Task Then_Other_Unapproved_Courses_Are_Left_Alone_When_More_Than_One_Exists()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();
        var firstOtherLearning = CreateLearning(isApproved: false);
        var secondOtherLearning = CreateLearning(isApproved: false);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        _learningRepository
            .Setup(x => x.GetOtherUnapprovedCourseLearnings(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel> { firstOtherLearning, secondOtherLearning });

        // Act
        var result = await _handler.Handle(command);

        // Assert
        firstOtherLearning.LatestEpisode.IsRemoved.Should().BeFalse();
        secondOtherLearning.LatestEpisode.IsRemoved.Should().BeFalse();
        _learningRepository.Verify(x => x.Update(firstOtherLearning), Times.Never);
        _learningRepository.Verify(x => x.Update(secondOtherLearning), Times.Never);
        result!.RemovedLearningKey.Should().BeNull();
    }

    [Test]
    public async Task Then_Nothing_Happens_When_No_Other_Unapproved_Courses_Exist()
    {
        // Arrange
        var command = CreateCommand();
        var learner = CreateLearner();

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetAllByLearnerKey(learner.Key, command.Ukprn, command.TrainingCode))
            .ReturnsAsync(new List<ApprenticeshipLearningDomainModel>());

        // Act
        var result = await _handler.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result!.RemovedLearningKey.Should().BeNull();
    }

    private void AssertPersonalDetailsEvent(
        LearnerDomainModel domainModel,
        long approvalsApprenticeshipId,
        Guid learningKey,
        string firstName,
        string lastName)
    {
        var domainEvent = domainModel.FlushEvents().OfType<Domain.Events.PersonalDetailsChangedEvent>().SingleOrDefault();

        domainEvent.Should().NotBeNull();

        domainEvent!.ApprovalsApprenticeshipId.Should().Be(approvalsApprenticeshipId);
        domainEvent.LearningKey.Should().Be(learningKey);
        domainEvent.FirstName.Should().Be(firstName);
        domainEvent.LastName.Should().Be(lastName);

    }

    private CreateDraftApprenticeshipLearningCommand CreateCommand()
    {
        var firstName = "UpdatedFirstName";
        var lastName = "UpdatedLastName";
        var dateOfBirth = new DateTime(2000, 1, 1);

        var model = new LearningUpdateContext
        {
            ApprovalsApprenticeshipId = _fixture.Create<long>(),
            Learner = new LearnerModel
            {
                Uln = "1234567890",
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = "test@example.com"
            },
            Care = new CareDetails
            {
                HasEHCP = true,
                IsCareLeaver = true,
                CareLeaverEmployerConsentGiven = true
            },
            Delivery = new DeliveryDetails
            {
                WithdrawalDate = null
            },
            Learning = new LearningUpdateDetails
            {
                CompletionDate = null
            },
            EnglishAndMathsCourses =
            [
                new EnglishAndMathsUpdateDetails
                {
                    Course = "ST0001",
                    LearnAimRef = "ENG001",
                    StartDate = new DateTime(2025, 8, 1),
                    PlannedEndDate = new DateTime(2026, 7, 31),
                    Amount = 100,
                    BreaksInLearning = []
                }
            ],
            LearningSupport =
            [
                new LearningSupportDetails
                {
                    StartDate = new DateTime(2025, 8, 1),
                    EndDate = new DateTime(2026, 7, 31)
                }
            ],
            OnProgrammeDetails = new OnProgrammeDetails
            {
                ExpectedEndDate = new DateTime(2026, 7, 31),
                AchievementDate = null,
                PauseDate = null,
                BreaksInLearning = [],
                Costs =
                [
                    new Cost
                    {
                        FromDate = new DateTime(2025, 8, 1),
                        TrainingPrice = 1000,
                        EpaoPrice = 200
                    }
                ]
            }
        };

        return new CreateDraftApprenticeshipLearningCommand(Ukprn, model, TrainingCode);
    }

    private LearnerDomainModel CreateLearner()
    {
        var entity = _fixture.Build<Learner>()
            .With(x => x.Uln, "1234567890")
            .With(x => x.FirstName, "OriginalFirstName")
            .With(x => x.LastName, "OriginalLastName")
            .With(x => x.DateOfBirth, new DateTime(1999, 1, 1))
            .Create();

        return LearnerDomainModel.Get(entity);
    }

    private ApprenticeshipLearningDomainModel CreateLearning(bool isApproved, bool isRemoved = false)
    {
        var price = new EpisodePrice
        {
            Key = Guid.NewGuid(),
            EpisodeKey = Guid.NewGuid(),
            StartDate = new DateTime(2025, 8, 1),
            EndDate = new DateTime(2026, 7, 31),
            TrainingPrice = 1000,
            EndPointAssessmentPrice = 200,
            TotalPrice = 1200
        };

        var episode = new ApprenticeshipEpisode
        {
            Key = Guid.NewGuid(),
            LearningKey = Guid.NewGuid(),
            ApprovalsApprenticeshipId = _fixture.Create<long>(),
            Ukprn = Ukprn,
            EmployerAccountId = 100,
            EmployerType = EmployerType.Levy,
            FundingPlatform = FundingPlatform.SLD,
            LegalEntityName = "Test",
            TrainingCode = TrainingCode,
            IsApproved = isApproved,
            IsRemoved = isRemoved,
            Prices = new List<EpisodePrice> { price },
            LearningSupport = new List<ApprenticeshipLearningSupport>(),
            BreaksInLearning = new List<EpisodeBreakInLearning>()
        };

        price.EpisodeKey = episode.Key;

        var entity = new ApprenticeshipLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = Guid.NewGuid(),
            Episodes = new List<ApprenticeshipEpisode> { episode },
            EnglishAndMathsCourses = new List<EnglishAndMaths>()
        };

        episode.LearningKey = entity.Key;

        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}
