﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreatePriceChange;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.Infrastructure.Services;
using SFA.DAS.Learning.TestHelpers;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;
using SFA.DAS.Learning.Types;

namespace SFA.DAS.Learning.Command.UnitTests.CreatePriceChange;

[TestFixture]
public class WhenAPriceChangeIsCreated
{
    private CreatePriceChangeCommandHandler _commandHandler = null!;
    private Mock<ILearningRepository> _apprenticeshipRepository = null!;
    private Mock<IMessageSession> _messageSession = null!;
    private Mock<ISystemClockService> _systemClockService = null!;
    private DateTime _createdDate = DateTime.UtcNow;
    private Mock<ILogger<CreatePriceChangeCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _apprenticeshipRepository = new Mock<ILearningRepository>();
        _messageSession = new Mock<IMessageSession>();
        _systemClockService = new Mock<ISystemClockService>();
        _systemClockService.Setup(x => x.UtcNow).Returns(_createdDate);
        _logger = new Mock<ILogger<CreatePriceChangeCommandHandler>>();
        _commandHandler = new CreatePriceChangeCommandHandler(
            _apprenticeshipRepository.Object, _messageSession.Object, _systemClockService.Object, _logger.Object);

        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public async Task ThenPriceHistoryIsAddedToApprenticeship()
    {
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        var command = _fixture.Create<CreatePriceChangeCommand>();
        command.Initiator = ChangeInitiator.Provider.ToString();
        command.EffectiveFromDate = apprenticeship.LatestPrice.StartDate.AddDays(_fixture.Create<int>());
        _apprenticeshipRepository.Setup(x => x.Get(command.LearningKey)).ReturnsAsync(apprenticeship);
        
        await _commandHandler.Handle(command);
        
        _apprenticeshipRepository.Verify(x => x.Update(It.Is<LearningDomainModel>(y => y.GetEntity().PriceHistories.Count == 1)));
    }

    [TestCase("Provider")]
    [TestCase("Employer")]
    public async Task ThenCorrectPriceHistoryValuesAreSet(string initiator)
    {
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
        var command = _fixture.Create<CreatePriceChangeCommand>();
        command.Initiator = initiator;
        command.AssessmentPrice = apprenticeship.LatestPrice.GetEntity().EndPointAssessmentPrice + 1;
        command.TrainingPrice = apprenticeship.LatestPrice.GetEntity().TrainingPrice + 1;
        command.TotalPrice = (decimal)(command.TrainingPrice! + command.AssessmentPrice!);
        command.EffectiveFromDate = apprenticeship.LatestPrice.StartDate.AddDays(_fixture.Create<int>());

        apprenticeship.LatestPrice.GetEntity().TotalPrice = command.TotalPrice - 1;

        _apprenticeshipRepository.Setup(x => x.Get(command.LearningKey)).ReturnsAsync(apprenticeship);

        await _commandHandler.Handle(command);

        if (initiator == ChangeInitiator.Provider.ToString())
            _apprenticeshipRepository.Verify(x => x.Update(It.Is<LearningDomainModel>(y =>
                y.GetEntity().PriceHistories.Single().TrainingPrice == command.TrainingPrice &&
                y.GetEntity().PriceHistories.Single().AssessmentPrice == command.AssessmentPrice &&
                y.GetEntity().PriceHistories.Single().TotalPrice == command.TotalPrice &&
                y.GetEntity().PriceHistories.Single().EffectiveFromDate == command.EffectiveFromDate &&
                y.GetEntity().PriceHistories.Single().CreatedDate != DateTime.MinValue &&
                y.GetEntity().PriceHistories.Single().PriceChangeRequestStatus == ChangeRequestStatus.Created &&
                y.GetEntity().PriceHistories.Single().ProviderApprovedBy == command.UserId &&
                y.GetEntity().PriceHistories.Single().EmployerApprovedBy == null &&
                y.GetEntity().PriceHistories.Single().Initiator == ChangeInitiator.Provider
            )));
        else
            _apprenticeshipRepository.Verify(x => x.Update(It.Is<LearningDomainModel>(y =>
                y.GetEntity().PriceHistories.Single().TrainingPrice == command.TrainingPrice &&
                y.GetEntity().PriceHistories.Single().AssessmentPrice == command.AssessmentPrice &&
                y.GetEntity().PriceHistories.Single().TotalPrice == command.TotalPrice &&
                y.GetEntity().PriceHistories.Single().EffectiveFromDate == command.EffectiveFromDate &&
                y.GetEntity().PriceHistories.Single().CreatedDate != DateTime.MinValue &&
                y.GetEntity().PriceHistories.Single().PriceChangeRequestStatus == ChangeRequestStatus.Created &&
                y.GetEntity().PriceHistories.Single().ProviderApprovedBy == null &&
                y.GetEntity().PriceHistories.Single().EmployerApprovedBy == command.UserId &&
                y.GetEntity().PriceHistories.Single().Initiator == ChangeInitiator.Employer
            )));
    }

		[TestCase(5000, 5001, false)]
		[TestCase(5000, 5000, true)]
		[TestCase(5000, 4999, true)]
		public async Task ThenPriceChangeIsAutoApprovedCorrectly(decimal oldTotal, decimal newTotal, bool expectAutoApprove)
		{
			var apprenticeship = _fixture.Create<LearningDomainModel>();
        ApprenticeshipDomainModelTestHelper.AddEpisode(apprenticeship);
			var command = _fixture.Create<CreatePriceChangeCommand>();
			command.Initiator = ChangeInitiator.Provider.ToString();
			command.AssessmentPrice = newTotal*0.25m;
			command.TrainingPrice = newTotal - command.AssessmentPrice;
			command.TotalPrice = (decimal)(command.TrainingPrice! + command.AssessmentPrice!);
        command.EffectiveFromDate = apprenticeship.LatestPrice.StartDate.AddDays(_fixture.Create<int>());

			apprenticeship.LatestPrice.GetEntity().TotalPrice = oldTotal;

			_apprenticeshipRepository.Setup(x => x.Get(command.LearningKey)).ReturnsAsync(apprenticeship);

			await _commandHandler.Handle(command);

        if (expectAutoApprove)
        {
            _apprenticeshipRepository.Verify(x => x.Update(It.Is<LearningDomainModel>(y =>
                y.GetEntity().PriceHistories.Single().PriceChangeRequestStatus == ChangeRequestStatus.Approved)));

            AssertEventPublished(apprenticeship, command.EffectiveFromDate);
        }
        else
        {
            _apprenticeshipRepository.Verify(x => x.Update(It.Is<LearningDomainModel>(y =>
                y.GetEntity().PriceHistories.Single().PriceChangeRequestStatus == ChangeRequestStatus.Approved)), Times.Never);
        }
    }

		[Test]
    public void ThenAnExceptionIsThrownIfTheRequesterIsNotSet()
    {
        var command = _fixture.Create<CreatePriceChangeCommand>();
        command.Initiator = string.Empty;
            
        var apprenticeship = _fixture.Create<LearningDomainModel>();

        _apprenticeshipRepository.Setup(x => x.Get(command.LearningKey)).ReturnsAsync(apprenticeship);

        Assert.ThrowsAsync<ArgumentException>(() => _commandHandler.Handle(command));
    }

    private void AssertEventPublished(LearningDomainModel learning, DateTime effectiveFromDate)
    {
        _messageSession.Verify(x => x.Publish(It.Is<LearningPriceChangedEvent>(e =>
            DoApprenticeshipDetailsMatchDomainModel(e, learning) &&
            e.ApprovedDate == _createdDate &&
            e.ApprovedBy == ApprovedBy.Provider &&
            e.EffectiveFromDate == effectiveFromDate &&
            ApprenticeshipDomainModelTestHelper.DoEpisodeDetailsMatchDomainModel(e, learning)), It.IsAny<PublishOptions>(),
            It.IsAny<CancellationToken>()));
    }

    private static bool DoApprenticeshipDetailsMatchDomainModel(LearningPriceChangedEvent e, LearningDomainModel learning)
    {
        return
            e.LearningKey == learning.Key &&
            e.ApprovalsApprenticeshipId == learning.ApprovalsApprenticeshipId;
    }
}
