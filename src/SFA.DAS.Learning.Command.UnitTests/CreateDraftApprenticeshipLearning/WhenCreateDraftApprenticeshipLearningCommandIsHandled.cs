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
    private const long _Ukprn = 12345678;
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
    }

    [Test]
    public async Task Then_New_Learner_And_Learning_Are_Created_When_Learner_Does_Not_Exist()
    {
        var command = CreateCommand();
        ApprenticeshipLearningDomainModel? addedLearning = null;

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .Returns(Task.FromResult<LearnerDomainModel?>(null));

        _learningRepository
            .Setup(x => x.GetByLearnerKey(It.IsAny<Guid>()))
            .ReturnsAsync((ApprenticeshipLearningDomainModel?)null);

        _learningRepository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(l => addedLearning = l)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command);

        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Add(It.IsAny<LearnerDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        addedLearning.Should().NotBeNull();
        addedLearning!.LatestEpisode.IsApproved.Should().BeFalse();
        addedLearning.LatestEpisode.EmployerAccountId.Should().BeNull();
        addedLearning.LatestEpisode.FundingType.Should().Be(FundingType.Levy);
    }

    [Test]
    public async Task Then_New_Learning_Is_Created_When_Learner_Exists_But_Learning_Does_Not_Exist()
    {
        var command = CreateCommand();
        var learner = CreateLearner();
        ApprenticeshipLearningDomainModel? addedLearning = null;

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .Returns(Task.FromResult<LearnerDomainModel?>(learner));

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .Returns(Task.FromResult<ApprenticeshipLearningDomainModel?>(null));

        _learningRepository
            .Setup(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()))
            .Callback<ApprenticeshipLearningDomainModel>(l => addedLearning = l)
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command);

        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Add(It.IsAny<LearnerDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Once);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        addedLearning.Should().NotBeNull();
        addedLearning!.LatestEpisode.IsApproved.Should().BeFalse();
    }

    [Test]
    public async Task Then_Null_Is_Returned_When_Latest_Episode_Is_Not_Removed()
    {
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Active);


        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        var result = await _handler.Handle(command);

        result.Should().BeNull();
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        _learningRepository.Verify(x => x.Update(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
    }

    [Test]
    public async Task Then_Learning_And_Learner_Are_Updated_When_Reinstating()
    {
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        var result = await _handler.Handle(command);

        result.Should().NotBeNull();
        _learnerRepository.Verify(x => x.Update(learner), Times.Once);
        _learningRepository.Verify(x => x.Update(learning), Times.Once);
        _learningRepository.Verify(x => x.Add(It.IsAny<ApprenticeshipLearningDomainModel>()), Times.Never);
        learning.LatestEpisode.IsApproved.Should().BeFalse();
    }

    [Test]
    public async Task Then_Result_Is_Returned_When_Reinstating_Learning()
    {
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        var result = await _handler.Handle(command);

        result.Should().NotBeNull();
        result!.LearningKey.Should().Be(learning.Key);
        result.LearningEpisodeKey.Should().Be(learning.LatestEpisode.Key);
    }

    [Test]
    public async Task Then_PersonalDetailsChangedEvent_Is_Added_When_Personal_Details_Have_Changed()
    {
        var command = CreateCommand();
        var learner = CreateLearner();
        var learning = CreateLearning(EpisodeStatus.Removed);

        _learnerRepository
            .Setup(x => x.GetByUln(It.IsAny<string>()))
            .ReturnsAsync(learner);

        _learningRepository
            .Setup(x => x.GetByLearnerKey(learner.Key))
            .ReturnsAsync(learning);

        await _handler.Handle(command);

        AssertPersonalDetailsEvent(learner, learning.LatestEpisode.ApprovalsApprenticeshipId, learning.Key, command.LearningUpdateContext.Learner.FirstName, command.LearningUpdateContext.Learner.LastName);
    }

    private void AssertPersonalDetailsEvent(
        LearnerDomainModel domainModel,
        long approvalsApprenticeshipId,
        Guid learningKey,
        string firstName,
        string lastName)
    {
        var domainEvent = domainModel.FlushEvents().OfType<SFA.DAS.Learning.Domain.Events.PersonalDetailsChangedEvent>().SingleOrDefault();

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
            Care = new Models.UpdateModels.Shared.CareDetails
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
                new Models.UpdateModels.Shared.LearningSupportDetails
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

        return new CreateDraftApprenticeshipLearningCommand(_Ukprn, model);
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

    private enum EpisodeStatus
    {
        Active,
        Removed
    }

    private ApprenticeshipLearningDomainModel CreateLearning(EpisodeStatus episodeStatus)
    {
        var price = new SFA.DAS.Learning.DataAccess.Entities.Learning.EpisodePrice
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
            Ukprn = _Ukprn,
            EmployerAccountId = 100,
            FundingType = FundingType.Levy,
            FundingPlatform = FundingPlatform.SLD,
            LegalEntityName = "Test",
            TrainingCode = "ST0001",
            IsApproved = true,
            IsRemoved = episodeStatus == EpisodeStatus.Removed,
            Prices = new List<SFA.DAS.Learning.DataAccess.Entities.Learning.EpisodePrice> { price },
            LearningSupport = new List<ApprenticeshipLearningSupport>(),
            BreaksInLearning = new List<SFA.DAS.Learning.DataAccess.Entities.Learning.EpisodeBreakInLearning>()
        };

        price.EpisodeKey = episode.Key;

        var entity = new ApprenticeshipLearning
        {
            Key = Guid.NewGuid(),
            LearnerKey = Guid.NewGuid(),
            Episodes = new List<ApprenticeshipEpisode> { episode },
            EnglishAndMathsCourses = new List<SFA.DAS.Learning.DataAccess.Entities.Learning.EnglishAndMaths>()
        };

        episode.LearningKey = entity.Key;

        return ApprenticeshipLearningDomainModel.Get(entity);
    }
}