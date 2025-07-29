using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.UnitTests.Helpers;
using SFA.DAS.Learning.TestHelpers;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository;

public class WhenGettingLearnerStatus
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
    public async Task ThenReturnNullWhenNoApprenticeshipFoundWithKey()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();

        // Act
        var result = await _sut.GetLearnerStatus(_fixture.Create<Guid>());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ThenTheCorrectLearnerStatusIsReturned()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();
        var apprenticeshipKey = _fixture.Create<Guid>();

        var apprenticeship = await _dbContext.AddApprenticeship(apprenticeshipKey, false, learnerStatus: LearnerStatus.Active);

        // Act
        var result = await _sut.GetLearnerStatus(apprenticeshipKey);

        // Assert
        result.Should().NotBeNull();
        result?.LearnerStatus.Should().Be(Enum.Parse<LearnerStatus>(apprenticeship.Episodes.First().LearningStatus));
    }

    [Test]
    public async Task ThenTheCorrectLearnerStatusIsReturnedWhenWithdrawn()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();
        var apprenticeshipKey = _fixture.Create<Guid>();

        var apprenticeship = await _dbContext.AddApprenticeship(apprenticeshipKey, false, learnerStatus: LearnerStatus.Withdrawn, addWithdrawalRequest: true);

        // Act
        var result = await _sut.GetLearnerStatus(apprenticeshipKey);

        // Assert
        result.Should().NotBeNull();
        result?.LearnerStatus.Should().Be(Enum.Parse<LearnerStatus>(apprenticeship.Episodes.First().LearningStatus));
        result?.WithdrawalChangedDate.Should().Be(apprenticeship.WithdrawalRequests.First().CreatedDate);
        result?.WithdrawalReason.Should().Be(apprenticeship.WithdrawalRequests.First().Reason);
        result?.LastDayOfLearning.Should().Be(apprenticeship.WithdrawalRequests.First().LastDayOfLearning);
    }

    private void SetUpApprenticeshipQueryRepository()
    {
        _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
        var logger = Mock.Of<ILogger<LearningQueryRepository>>();
        _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
    }
}