using System.Linq;
using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.TestHelpers.AutoFixture.Customizations;

namespace SFA.DAS.Learning.Domain.UnitTests.Learning;

[TestFixture]
public class WhenAPriceChangeIsCreated
{
    private Fixture _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new Fixture();
        _fixture.Customize(new ApprenticeshipCustomization());
    }

    [Test]
    public void ThenThePriceHistoryRecordIsAdded()
    {
        var apprenticeship = _fixture.Create<LearningDomainModel>();
        var expectedModel = PriceHistoryDomainModel.Get(_fixture.Create<PriceHistory>());
        apprenticeship.AddPriceHistory(
            expectedModel.TrainingPrice,
            expectedModel.AssessmentPrice,
            expectedModel.TotalPrice,
            expectedModel.EffectiveFromDate,
            expectedModel.CreatedDate,
            expectedModel.PriceChangeRequestStatus,
            expectedModel.ProviderApprovedBy,
            expectedModel.ChangeReason!,
            null,
            expectedModel.ProviderApprovedDate,
            expectedModel.EmployerApprovedDate,
            expectedModel.Initiator);

        var priceHistory = apprenticeship.GetEntity().PriceHistories.Single(x => x.EffectiveFromDate == expectedModel.EffectiveFromDate);

        priceHistory.Should()
            .BeEquivalentTo(expectedModel,
                opts => opts.Excluding(x => x.Key)
                    .Excluding(x => x.ApprenticeshipKey)
                    .Excluding(x => x.ProviderApprovedBy)
                    .Excluding(x => x.ProviderApprovedDate)
                    .Excluding(x => x.EmployerApprovedBy)
                    .Excluding(x => x.EmployerApprovedDate));
    }
}