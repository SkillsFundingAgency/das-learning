using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Episode = SFA.DAS.Learning.DataAccess.Entities.Learning.Episode;
using EpisodePrice = SFA.DAS.Learning.DataAccess.Entities.Learning.EpisodePrice;

namespace SFA.DAS.Learning.Domain.UnitTests.Repositories.ApprenticeshipQueryRepository;

public class WhenGettingApprenticeshipsWithEpisodes
{
    private LearningQueryRepository _sut = null!;
    private Fixture _fixture = null!;
    private LearningDataContext _dbContext = null!;

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
    public async Task ThenReturnEmptyListWhenNoApprenticeshipsFoundForUkprn()
    {
        //Arrange
        SetUpApprenticeshipQueryRepository();

        //Act
        var result = await _sut.GetLearningsWithEpisodes(_fixture.Create<long>());

        //Assert
        result.Data.Should().BeEmpty();
    }

    [Test]
    public async Task ThenTheCorrectDataIsReturned()
    {
        //Arrange
        SetUpApprenticeshipQueryRepository();

        var apprenticeshipKey = _fixture.Create<Guid>();
        var episode1Key = _fixture.Create<Guid>();
        var episode2Key = _fixture.Create<Guid>();

        var ukprn = _fixture.Create<long>();
        var startDate = _fixture.Create<DateTime>();
        var ageAtStartOfApprenticeship = 20;
        var endDate = startDate.AddYears(2);
        var trainingCode = _fixture.Create<string>();

        var episodePrice1 = CreateEpisodePrice(episode1Key, startDate, startDate.AddDays(1));
        var episodePrice2 = CreateEpisodePrice(episode1Key, startDate.AddDays(1), startDate.AddMonths(8));
        var episode1 = CreateEpisode(episode1Key, ukprn, trainingCode, episodePrice1, episodePrice2);

        var episodePrice3 = CreateEpisodePrice(episode2Key, startDate.AddMonths(8), startDate.AddYears(1));
        var episodePrice4 = CreateEpisodePrice(episode2Key, startDate.AddYears(1), endDate);
        var episode2 = CreateEpisode(episode2Key, ukprn, trainingCode, episodePrice3, episodePrice4);

        var apprenticeshipRecord = _fixture.Build<DataAccess.Entities.Learning.Learning>()
                .With(x => x.Key, apprenticeshipKey)
                .With(x => x.Episodes, new List<Episode>() { episode1, episode2 })
                .With(x => x.DateOfBirth, startDate.AddYears(-20).AddMonths(-6))
                .With(x => x.Uln, _fixture.Create<long>().ToString())
                .Create();

        await _dbContext.AddRangeAsync(new[] { apprenticeshipRecord });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetLearningsWithEpisodes(ukprn);

        // Assert
        result.Should().NotBeNull();
        var apprenticeship = result.Data.SingleOrDefault();
        AssertApprenticeship(apprenticeshipRecord, startDate, endDate, ageAtStartOfApprenticeship, apprenticeship);

        var resultEpisode1 = apprenticeship.Episodes.SingleOrDefault(x => x.Key == episode1Key);
        AssertEpisode(episode1, resultEpisode1);
        resultEpisode1.Prices.Should().Contain(x => AssertPrice(episodePrice1, x));
        resultEpisode1.Prices.Should().Contain(x => AssertPrice(episodePrice2, x));


        var resultEpisode2 = apprenticeship.Episodes.SingleOrDefault(x => x.Key == episode2Key);
        AssertEpisode(episode2, resultEpisode2);
        resultEpisode2.Prices.Should().Contain(x => AssertPrice(episodePrice3, x));
        resultEpisode2.Prices.Should().Contain(x => AssertPrice(episodePrice4, x));
    }

    [Test]
    public async Task ThenCompletionDataReturnedWhenCompletionExists()
    {
        //Arrange
        SetUpApprenticeshipQueryRepository();

        var apprenticeshipKey = _fixture.Create<Guid>();
        var episodeKey = _fixture.Create<Guid>();

        var ukprn = _fixture.Create<long>();
        var startDate = _fixture.Create<DateTime>();
        var endDate = startDate.AddYears(2);
        var trainingCode = _fixture.Create<string>();
        var completionDate = startDate.AddYears(1);

        var episodePrice = CreateEpisodePrice(episodeKey, startDate, endDate);
        var episode = CreateEpisode(episodeKey, ukprn, trainingCode, episodePrice);

        var apprenticeshipRecord = _fixture.Build<DataAccess.Entities.Learning.Learning>()
                .With(x => x.Key, apprenticeshipKey)
                .With(x => x.Episodes, new List<Episode>() { episode })
                .With(x => x.DateOfBirth, startDate.AddYears(-20).AddMonths(-6))
                .With(x => x.Uln, _fixture.Create<long>().ToString())
                .With(x => x.CompletionDate, completionDate)
                .Create();

        await _dbContext.AddRangeAsync(new[] { apprenticeshipRecord });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetLearningsWithEpisodes(ukprn);

        // Assert
        result.Should().NotBeNull();
        var apprenticeship = result.Data.SingleOrDefault();
        apprenticeship.CompletionDate.Should().Be(completionDate);
    }

    [Test]
    public async Task ThenApprenticeshipIsReturnedWhenActiveOnGivenDate()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();

        var apprenticeshipKey = _fixture.Create<Guid>();
        var episodeKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();
        var startDate = new DateTime(2025, 8, 1);
        var endDate = new DateTime(2026, 7, 31);
        var activeDate = new DateTime(2025, 12, 1); // within the range
        var trainingCode = _fixture.Create<string>();

        var episodePrice = CreateEpisodePrice(episodeKey, startDate, endDate);
        var episode = CreateEpisode(episodeKey, ukprn, trainingCode, episodePrice);

        var apprenticeshipRecord = _fixture.Build<DataAccess.Entities.Learning.Learning>()
            .With(x => x.Key, apprenticeshipKey)
            .With(x => x.Episodes, new List<Episode> { episode })
            .With(x => x.DateOfBirth, startDate.AddYears(-20))
            .With(x => x.Uln, _fixture.Create<long>().ToString())
            .Create();

        await _dbContext.AddRangeAsync(new[] { apprenticeshipRecord });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetLearningsWithEpisodes(ukprn, activeDate);

        // Assert
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Single().Key.Should().Be(apprenticeshipKey);
    }

    [Test]
    public async Task ThenApprenticeshipIsNotReturnedWhenNotActiveOnGivenDate()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();

        var apprenticeshipKey = _fixture.Create<Guid>();
        var episodeKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();
        var startDate = new DateTime(2023, 8, 1);
        var endDate = new DateTime(2024, 7, 31);
        var activeDate = new DateTime(2025, 12, 1); // after end date
        var trainingCode = _fixture.Create<string>();

        var episodePrice = CreateEpisodePrice(episodeKey, startDate, endDate);
        var episode = CreateEpisode(episodeKey, ukprn, trainingCode, episodePrice);

        var apprenticeshipRecord = _fixture.Build<DataAccess.Entities.Learning.Learning>()
            .With(x => x.Key, apprenticeshipKey)
            .With(x => x.Episodes, new List<Episode> { episode })
            .With(x => x.DateOfBirth, startDate.AddYears(-20))
            .With(x => x.Uln, _fixture.Create<long>().ToString())
            .Create();

        await _dbContext.AddAsync(apprenticeshipRecord);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetLearningsWithEpisodes(ukprn, activeDate);

        // Assert
        result.Data.Should().BeEmpty("because the episode ended before the active date");
    }

    [Test]
    public async Task ThenPaginationIsAppliedWhenSpecified()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();

        var ukprn = _fixture.Create<long>();
        var apprenticeships = new List<DataAccess.Entities.Learning.Learning>();

        // Create 5 apprenticeships
        for (int i = 1; i <= 5; i++)
        {
            var apprenticeshipKey = _fixture.Create<Guid>();
            var episodeKey = _fixture.Create<Guid>();
            var startDate = _fixture.Create<DateTime>();
            var endDate = startDate.AddYears(1);
            var trainingCode = $"TC{i}";

            var episodePrice = CreateEpisodePrice(episodeKey, startDate, endDate);
            var episode = CreateEpisode(episodeKey, ukprn, trainingCode, episodePrice);

            var apprenticeship = _fixture.Build<DataAccess.Entities.Learning.Learning>()
                .With(x => x.Key, apprenticeshipKey)
                .With(x => x.Episodes, new List<Episode> { episode })
                .With(x => x.DateOfBirth, startDate.AddYears(-20))
                .With(x => x.Uln, i.ToString()) // Ensure Uln ordering
                .Create();

            apprenticeships.Add(apprenticeship);
        }

        await _dbContext.AddRangeAsync(apprenticeships);
        await _dbContext.SaveChangesAsync();

        // Getting page 2 with page size of 2
        int pageSize = 2;
        int pageOffset = 2;

        // Act
        var result = await _sut.GetLearningsWithEpisodes(
            ukprn,
            limit: pageSize,
            offset: pageOffset);

        // Assert
        result.Should().NotBeNull();
        result.Data.Count().Should().Be(pageSize);
        result.TotalItems.Should().Be(apprenticeships.Count);
        result.TotalPages.Should().Be((int)Math.Ceiling((double)apprenticeships.Count / pageSize));

        var expectedUlnsForPage = apprenticeships
            .OrderBy(a => a.Uln)
            .Skip(pageOffset)
            .Take(pageSize)
            .Select(a => a.Uln)
            .ToList();

        result.Data.Select(a => a.Uln).Should().BeEquivalentTo(expectedUlnsForPage);
    }

    [Test]
    public async Task ThenApprenticeshipIsNotReturnedWhenLastDayOfLearningIsBeforeActiveOnDate()
    {
        // Arrange
        SetUpApprenticeshipQueryRepository();

        var apprenticeshipKey = _fixture.Create<Guid>();
        var episodeKey = _fixture.Create<Guid>();
        var ukprn = _fixture.Create<long>();
        var trainingCode = _fixture.Create<string>();

        var startDate = new DateTime(2025, 8, 1);
        var endDate = new DateTime(2026, 7, 31);
        var activeOnDate = new DateTime(2025, 12, 1);

        var lastDayOfLearning = new DateTime(2025, 11, 30);

        var episodePrice = CreateEpisodePrice(episodeKey, startDate, endDate);

        var episode = CreateEpisode(episodeKey, ukprn, trainingCode, episodePrice);
        episode.LastDayOfLearning = lastDayOfLearning;

        var apprenticeshipRecord = _fixture.Build<DataAccess.Entities.Learning.Learning>()
            .With(x => x.Key, apprenticeshipKey)
            .With(x => x.Episodes, new List<Episode> { episode })
            .With(x => x.DateOfBirth, startDate.AddYears(-22))
            .With(x => x.Uln, _fixture.Create<long>().ToString())
            .Create();

        await _dbContext.AddAsync(apprenticeshipRecord);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetLearningsWithEpisodes(ukprn, activeOnDate);

        // Assert
        result.Data.Should().BeEmpty("because LastDayOfLearning is before the active on date");
    }

    private void AssertApprenticeship(
        DataAccess.Entities.Learning.Learning expected,
        DateTime startDate,
        DateTime endDate,
        int age,
        LearningWithEpisodes actual)
    {
        actual.Should().NotBeNull();
        actual.StartDate.Should().Be(startDate);
        actual.AgeAtStartOfApprenticeship.Should().Be(age);
        actual.Key.Should().Be(expected.Key);
        actual.PlannedEndDate.Should().Be(endDate);
        actual.Uln.Should().Be(expected.Uln);
        actual.Episodes.Count.Should().Be(expected.Episodes.Count);
    }

    private void AssertEpisode(Episode expected, DataTransferObjects.Episode actual)
    {
        actual.Should().NotBeNull();
        actual.TrainingCode.Should().Be(expected.TrainingCode);
        actual.Prices.Count.Should().Be(expected.Prices.Count);
        actual.LastDayOfLearning.Should().Be(expected.LastDayOfLearning);
    }

    private bool AssertPrice(EpisodePrice expected, DataTransferObjects.EpisodePrice actual)
    {
        return actual.EndDate == expected.EndDate
            && actual.EndPointAssessmentPrice == expected.EndPointAssessmentPrice
            && actual.FundingBandMaximum == expected.FundingBandMaximum
            && actual.StartDate == expected.StartDate
            && actual.TotalPrice == expected.TotalPrice
            && actual.TrainingPrice == expected.TrainingPrice;
    }

    private void SetUpApprenticeshipQueryRepository()
    {
        _dbContext = InMemoryDbContextCreator.SetUpInMemoryDbContext();
        var logger = Mock.Of<ILogger<LearningQueryRepository>>();
        _sut = new LearningQueryRepository(new Lazy<LearningDataContext>(_dbContext), logger);
    }

    private EpisodePrice CreateEpisodePrice(Guid episodeKey, DateTime start, DateTime end)
    {
        return _fixture.Build<EpisodePrice>()
            .With(x => x.Key, _fixture.Create<Guid>())
            .With(x => x.EpisodeKey, episodeKey)
            .With(x => x.StartDate, start)
            .With(x => x.EndDate, end)
            .Create();
    }

    private Episode CreateEpisode(Guid key, long ukprn, string trainingCode, params EpisodePrice[] prices)
    {
        return _fixture.Build<Episode>()
            .With(x => x.Key, key)
            .With(x => x.Prices, prices.ToList())
            .With(x => x.Ukprn, ukprn)
            .With(x => x.TrainingCode, trainingCode)
            .With(x=> x.FundingPlatform, DAS.Learning.Enums.FundingPlatform.DAS)
            .Create();
    }
}