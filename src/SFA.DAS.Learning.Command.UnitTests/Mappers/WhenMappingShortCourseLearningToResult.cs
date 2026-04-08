using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Learning.Command.DeleteShortCourse;
using SFA.DAS.Learning.Command.Mappers;
using SFA.DAS.Learning.Command.UpdateShortCourse;
using SFA.DAS.Learning.DataAccess.Entities.Learning;
using SFA.DAS.Learning.Domain.Apprenticeship;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Learning.Command.UnitTests.Mappers;

[TestFixture]

public class WhenMappingShortCourseLearningToResult
{
    private ShortCourseLearningDomainModelMapper _mapper;

    [SetUp]
    public void SetUp()
    {
        _mapper = new ShortCourseLearningDomainModelMapper();
    }

    [Test]
    public void Map_ShouldMapToDeleteShortCourseResult()
    {
        // Arrange
        var learning = CreateLearning();
        var learner = CreateLearner();

        // Act
        var result = _mapper.Map<DeleteShortCourseResult>(learning, learner, 123);

        // Assert
        result.Should().BeOfType<DeleteShortCourseResult>();
        result.LearningKey.Should().Be(learning.Key);
        result.CompletionDate.Should().Be(learning.CompletionDate);

        result.Learner.Should().BeEquivalentTo(new
        {
            learner.Uln,
            learner.FirstName,
            learner.LastName,
            learner.DateOfBirth
        });
    }

    [Test]
    public void Map_ShouldMapToUpdateShortCourseResult()
    {
        var learning = CreateLearning();
        var learner = CreateLearner();

        var result = _mapper.Map<UpdateShortCourseResult>(learning, learner, 123);

        result.Should().BeOfType<UpdateShortCourseResult>();
    }

    [Test]
    public void Map_ShouldOnlyIncludeEpisodesMatchingUkprn()
    {
        var learning = CreateLearning();
        var learner = CreateLearner();

        var result = _mapper.Map<DeleteShortCourseResult>(learning, learner, 123);

        result.Episodes.Should().HaveCount(1);
        result.Episodes.Single().Ukprn.Should().Be(123);
    }

    [Test]
    public void Map_ShouldMapEpisodeFields()
    {
        var learning = CreateLearning();
        var learner = CreateLearner();

        var result = _mapper.Map<DeleteShortCourseResult>(learning, learner, 123);
        var episode = result.Episodes.Single();

        var source = learning.Episodes.Single(e => e.Ukprn == 123);

        episode.Should().BeEquivalentTo(new
        {
            source.Ukprn,
            source.EmployerAccountId,
            CourseCode = source.TrainingCode,
            CourseType = CourseTypeConstants.ShortCourse,
            source.LearningType,
            source.StartDate,
            PlannedEndDate = source.ExpectedEndDate,
            source.WithdrawalDate,
            source.IsApproved,
            source.Price,
            source.LearnerRef,
            source.EmployerType
        });
    }

    [Test]
    public void Map_ShouldCalculateAgeAtStart()
    {
        var learning = CreateLearning();
        var learner = CreateLearner();

        var expectedAge = learner.AgeOnDate(learning.Episodes.First().StartDate);

        var result = _mapper.Map<DeleteShortCourseResult>(learning, learner, 123);

        result.Episodes.Single().AgeAtStart.Should().Be(expectedAge);
    }

    private static ShortCourseLearningDomainModel CreateLearning()
    {
        var entity = new ShortCourseLearning
        {
            Key = Guid.NewGuid(),
            CompletionDate = new DateTime(2024, 1, 1),
            Episodes = new List<ShortCourseEpisode>
            {
                new ShortCourseEpisode
                {
                    Ukprn = 123,
                    EmployerAccountId = 1,
                    TrainingCode = "SC001",
                    LearningType = Enums.LearningType.ApprenticeshipUnit,
                    StartDate = new DateTime(2023, 1, 1),
                    ExpectedEndDate = new DateTime(2023, 6, 1),
                    WithdrawalDate = null,
                    IsApproved = true,
                    Price = 1500,
                    LearnerRef = "Ref1",
                    EmployerType = Enums.EmployerType.Levy
                }
            }
        };

        return ShortCourseLearningDomainModel.Get(entity);
    }

    private static LearnerDomainModel CreateLearner()
    {
        return LearnerDomainModel.Get(new Learner
        {
            Key = Guid.NewGuid(),
            Uln = "1234567890",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(2000, 1, 1)
        });
    }

}
