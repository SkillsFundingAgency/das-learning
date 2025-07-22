using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository
{
    public class WhenGettingKeyByApprenticeshipId
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
        public async Task ThenReturnNullWhenNoRecordFoundWithApprenticeshipId()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();

            //Act
            var result = await _sut.GetKeyByLearningId(_fixture.Create<long>());

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task ThenTheCorrectApprenticeshipKeyIsReturned()
        {
            //Arrange
            SetUpApprenticeshipQueryRepository();

            //Act
            var approvalsApprenticeshipId = _fixture.Create<long>();
            var expectedApprenticeshipKey = _fixture.Create<Guid>();
            await _dbContext.AddApprenticeship(expectedApprenticeshipKey, false, approvalsApprenticeshipId: approvalsApprenticeshipId);
            await _dbContext.AddApprenticeship(_fixture.Create<Guid>(), false, approvalsApprenticeshipId: _fixture.Create<long>());

            // Act
            var result = await _sut.GetKeyByLearningId(approvalsApprenticeshipId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(expectedApprenticeshipKey);
        }

        private DataAccess.Entities.Learning.Learning CreateApprenticeshipWithApproval(Guid apprenticeshipKey, long apprenticeshipId)
        {
            return _fixture.Build<DataAccess.Entities.Learning.Learning>()
                .With(x => x.Key, apprenticeshipKey)
                .Create();
        }

        private void SetUpApprenticeshipQueryRepository()
        {
            _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
            var logger = Mock.Of<ILogger<LearningQueryRepository>>();
            _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
        }
    }
}