using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenAPendingPriceChangeIsCancelled
{
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public void ThenThePriceHistoryRecordIsCancelled()
    {
        // Assert
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        var priceHistoryDomainModel = PriceHistoryDomainModel.Get(_fixture.Create<PriceHistory>());
        apprenticeship.AddPriceHistory(
            priceHistoryDomainModel.TrainingPrice,
            priceHistoryDomainModel.AssessmentPrice,
            priceHistoryDomainModel.TotalPrice,
            priceHistoryDomainModel.EffectiveFromDate,
            priceHistoryDomainModel.CreatedDate,
            priceHistoryDomainModel.PriceChangeRequestStatus,
            priceHistoryDomainModel.ProviderApprovedBy,
            priceHistoryDomainModel.ChangeReason!,
            null,
            priceHistoryDomainModel.ProviderApprovedDate,
            priceHistoryDomainModel.EmployerApprovedDate,
            priceHistoryDomainModel.Initiator);
        
        // Act
        apprenticeship.CancelPendingPriceChange();

        // Assert
        var priceHistory = apprenticeship.GetEntity().PriceHistories.Single(x => x.PriceChangeRequestStatus == ChangeRequestStatus.Cancelled);
        priceHistory.Should().NotBeNull();
    }
}