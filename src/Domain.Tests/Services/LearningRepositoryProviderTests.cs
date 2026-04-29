using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using System;

namespace SFA.DAS.Learning.Domain.UnitTests.Services;

public class LearningRepositoryProviderTests
{
    private Mock<IApprenticeshipLearningRepository> _apprenticeships = null!;
    private Mock<IShortCourseLearningRepository> _shortCourses = null!;
    private LearningRepositoryProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        _apprenticeships = new Mock<IApprenticeshipLearningRepository>();
        _shortCourses = new Mock<IShortCourseLearningRepository>();

        _provider = new LearningRepositoryProvider(
            _apprenticeships.Object,
            _shortCourses.Object);
    }

    [TestCase(LearningType.Apprenticeship)]
    [TestCase(LearningType.FoundationApprenticeship)]
    public void GetRepository_ByLearningType_ReturnsApprenticeshipRepository(LearningType type)
    {
        var repo = _provider.GetRepository(type);

        repo.Should().BeSameAs(_apprenticeships.Object);
    }

    [Test]
    public void GetRepository_ByLearningType_ApprenticeshipUnit_ReturnsShortCourseRepository()
    {
        var repo = _provider.GetRepository(LearningType.ApprenticeshipUnit);

        repo.Should().BeSameAs(_shortCourses.Object);
    }

    [Test]
    public void GetRepository_ByModel_ApprenticeshipModel_ReturnsApprenticeshipRepository()
    {
        var apprenticeshipLearningFactory = new ApprenticeshipLearningFactory();
        var model = apprenticeshipLearningFactory.CreateNew(Guid.NewGuid());

        var repo = _provider.GetRepository(model);

        repo.Should().BeSameAs(_apprenticeships.Object);
    }

    [Test]
    public void GetRepository_ByModel_ShortCourseModel_ReturnsShortCourseRepository()
    {
        var shortCourseLearningFactory = new ShortCourseLearningFactory();
        var model = shortCourseLearningFactory.CreateNew(Guid.NewGuid(), null);

        var repo = _provider.GetRepository(model);

        repo.Should().BeSameAs(_shortCourses.Object);
    }
}