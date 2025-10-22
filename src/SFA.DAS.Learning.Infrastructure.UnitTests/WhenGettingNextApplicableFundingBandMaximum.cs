using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient.Standards;
using SFA.DAS.Learning.Infrastructure.Services;

namespace SFA.DAS.Learning.Infrastructure.UnitTests;

public class WhenGettingNextApplicableFundingBandMaximum
{
    private Mock<IApprenticeshipsOuterApiClient> _apprenticeshipsOuterApiClient;
    private FundingBandMaximumService _service;
    private Fixture _fixture;

    [SetUp]
    public void Setup()
    {
        _fixture = new Fixture();
        _apprenticeshipsOuterApiClient = new Mock<IApprenticeshipsOuterApiClient>();
        _service = new FundingBandMaximumService(
            _apprenticeshipsOuterApiClient.Object,
            new Mock<ILogger<FundingBandMaximumService>>().Object);
    }

    [Test]
    public async Task ThenCorrectValueIsReturned_WhenDateFallsWithinEffectiveRange()
    {
        var courseCode = _fixture.Create<int>();
        var standard = _fixture.Create<GetStandardResponse>();

        standard.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        standard.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
        standard.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 31);
        standard.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 06, 01);
        standard.ApprenticeshipFunding[1].EffectiveTo = null;

        _apprenticeshipsOuterApiClient
            .Setup(x => x.GetStandard(courseCode))
            .ReturnsAsync(standard);

        var result = await _service.GetNextApplicableFundingBandMaximum(courseCode, new DateTime(2022, 05, 15));

        result.Should().Be(standard.ApprenticeshipFunding[0].MaxEmployerLevyCap);
    }

    [Test]
    public async Task ThenNextRecordIsUsed_WhenNoRecordCoversDate_ButNextStartsWithinSameMonth()
    {
        var courseCode = _fixture.Create<int>();
        var standard = _fixture.Create<GetStandardResponse>();

        standard.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        standard.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 03, 01);
        standard.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 03, 15);
        standard.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 03, 20); // same month
        standard.ApprenticeshipFunding[1].EffectiveTo = null;

        _apprenticeshipsOuterApiClient
            .Setup(x => x.GetStandard(courseCode))
            .ReturnsAsync(standard);

        var result = await _service.GetNextApplicableFundingBandMaximum(courseCode, new DateTime(2022, 03, 17));

        result.Should().Be(standard.ApprenticeshipFunding[1].MaxEmployerLevyCap);
    }

    [Test]
    public async Task ThenNullIsReturned_WhenNextRecordStartsInDifferentMonth()
    {
        var courseCode = _fixture.Create<int>();
        var standard = _fixture.Create<GetStandardResponse>();

        standard.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        standard.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 03, 01);
        standard.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 03, 15);
        standard.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 04, 01); // next month
        standard.ApprenticeshipFunding[1].EffectiveTo = null;

        _apprenticeshipsOuterApiClient
            .Setup(x => x.GetStandard(courseCode))
            .ReturnsAsync(standard);

        var result = await _service.GetNextApplicableFundingBandMaximum(courseCode, new DateTime(2022, 03, 20));

        result.Should().BeNull();
    }

    [Test]
    public async Task ThenNullIsReturned_WhenNoApplicableRecordExistsAtAll()
    {
        var courseCode = _fixture.Create<int>();
        var standard = _fixture.Create<GetStandardResponse>();

        standard.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
        standard.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
        standard.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 01, 31);
        standard.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 03, 01);
        standard.ApprenticeshipFunding[1].EffectiveTo = null;

        _apprenticeshipsOuterApiClient
            .Setup(x => x.GetStandard(courseCode))
            .ReturnsAsync(standard);

        var result = await _service.GetNextApplicableFundingBandMaximum(courseCode, new DateTime(2022, 02, 15));

        result.Should().BeNull();
    }
}