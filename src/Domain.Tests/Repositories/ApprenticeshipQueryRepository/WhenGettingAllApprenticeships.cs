using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.Enums;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository
{
    public class WhenGettingAllApprenticeships
    {
        private LearningQueryRepository _sut;
        private Fixture _fixture;
        private LearningDataContext _dbContext;

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
        }

        [TearDown]
        public void CleanUp()
        {
            _dbContext.Dispose();
        }

        [Test]
        public async Task ThenCorrectApprenticeshipsForUkprnAreRetrieved()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            SetUpApprenticeshipQueryRepository();
            var apprenticeship1 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);
            var apprenticeship2 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>());
            var apprenticeship3 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.SLD);
            var apprenticeship4 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);

            // Act
            var result = await _sut.GetAll(ukprn, null);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(3);
            result.Should().Contain(x => x.Uln == apprenticeship1.Uln);
            result.Should().NotContain(x => x.Uln == apprenticeship2.Uln);
            result.Should().Contain(x => x.Uln == apprenticeship3.Uln);
            result.Should().Contain(x => x.Uln == apprenticeship4.Uln);
        }

        [Test]
        public async Task AndFilteringByFundingPlatformSLDThenCorrectApprenticeshipsAreRetrieved()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            SetUpApprenticeshipQueryRepository();
            var apprenticeship1 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);
            var apprenticeship2 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>());
            var apprenticeship3 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.SLD);
            var apprenticeship4 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);

            // Act
            var result = await _sut.GetAll(ukprn, FundingPlatform.SLD);

            // Assert
            result.Count().Should().Be(1);
            result.Should().NotContain(x => x.Uln == apprenticeship1.Uln);
            result.Should().NotContain(x => x.Uln == apprenticeship2.Uln);
            result.Should().Contain(x => x.Uln == apprenticeship3.Uln);
            result.Should().NotContain(x => x.Uln == apprenticeship4.Uln);
        }

        [Test]
        public async Task AndFilteringByFundingPlatformDASThenCorrectApprenticeshipsAreRetrieved()
        {
            // Arrange
            var ukprn = _fixture.Create<long>();
            SetUpApprenticeshipQueryRepository();
            var apprenticeship1 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);
            var apprenticeship2 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>());
            var apprenticeship3 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.SLD);
            var apprenticeship4 = await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), ukprn, fundingPlatform: FundingPlatform.DAS);

            // Act
            var result = await _sut.GetAll(ukprn, FundingPlatform.DAS);

            // Assert
            result.Count().Should().Be(2);
            result.Should().Contain(x => x.Uln == apprenticeship1.Uln);
            result.Should().NotContain(x => x.Uln == apprenticeship2.Uln);
            result.Should().NotContain(x => x.Uln == apprenticeship3.Uln);
            result.Should().Contain(x => x.Uln == apprenticeship4.Uln);
        }

        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}
