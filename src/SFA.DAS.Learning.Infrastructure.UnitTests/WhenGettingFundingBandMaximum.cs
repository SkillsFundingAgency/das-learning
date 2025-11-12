using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;
using SFA.DAS.Learning.Infrastructure.Services;

namespace SFA.DAS.Learning.Infrastructure.UnitTests
{
    public class WhenGettingFundingBandMaximum
    {
        private Mock<ILearningOuterApiClient> _learningOuterApiClient;
        private FundingBandMaximumService _service;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
            _learningOuterApiClient = new Mock<ILearningOuterApiClient>();
            _service = new FundingBandMaximumService(_learningOuterApiClient.Object, new Mock<ILogger<FundingBandMaximumService>>().Object);
        }

        [Test]
        public async Task ThenNullIsReturnedForNullStartDate()
        {
            var courseCode = _fixture.Create<int>();
            var getStandardResponse = _fixture.Create<GetStandardResponse>();
            _learningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
            var result = await _service.GetFundingBandMaximum(courseCode, null);
            result.Should().BeNull();
        }

        [Test]
        public async Task ThenCorrectValueFromApiForPreviousFundingBandMaximumIsReturned()
        {
            var courseCode = _fixture.Create<int>();
            var getStandardResponse = _fixture.Create<GetStandardResponse>();
            getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
            getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
            getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
            _learningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
            var result = await _service.GetFundingBandMaximum(courseCode, new DateTime(2022, 01, 01));
            result.Should().Be(getStandardResponse.ApprenticeshipFunding[0].MaxEmployerLevyCap);
        }

        [Test]
        public async Task ThenCorrectValueFromApiForMostRecentFundingBandMaximumIsReturned()
        {
            var courseCode = _fixture.Create<int>();
            var getStandardResponse = _fixture.Create<GetStandardResponse>();
            getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
            getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
            getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
            _learningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
            var result = await _service.GetFundingBandMaximum(courseCode, new DateTime(2022, 05, 07));
            result.Should().Be(getStandardResponse.ApprenticeshipFunding[1].MaxEmployerLevyCap);
        }

        [Test]
        public async Task ThenNullIsReturnedWhenNoFundingBandForGivenDateExists()
        {
            var courseCode = _fixture.Create<int>();
            var getStandardResponse = _fixture.Create<GetStandardResponse>();
            var apprenticeshipKey = Guid.NewGuid();
            var actualStartDate = new DateTime(2021, 01, 01);
            getStandardResponse.ApprenticeshipFunding = _fixture.CreateMany<GetStandardFundingResponse>(2).ToList();
            getStandardResponse.ApprenticeshipFunding[0].EffectiveFrom = new DateTime(2022, 01, 01);
            getStandardResponse.ApprenticeshipFunding[0].EffectiveTo = new DateTime(2022, 05, 05);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveFrom = new DateTime(2022, 05, 06);
            getStandardResponse.ApprenticeshipFunding[1].EffectiveTo = null;
            _learningOuterApiClient.Setup(x => x.GetStandard(courseCode)).ReturnsAsync(getStandardResponse);
            var result = await _service.GetFundingBandMaximum(courseCode, actualStartDate);
            result.Should().BeNull();
        }
    }
}