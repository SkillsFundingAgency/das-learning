using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.UnitTests.Services;

public class LearningServiceTests
{
    private Mock<ILearningRepositoryProvider> _provider = null!;
    private Mock<ILearningRepository> _repo = null!;
    private LearningService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _provider = new Mock<ILearningRepositoryProvider>();
        _repo = new Mock<ILearningRepository>();

        _service = new LearningService(_provider.Object);
    }

    [Test]
    public async Task GetUnapprovedLearning_UsesRepositoryForLearningType()
    {
        // Arrange
        const string uln = "12345";
        const long approvalsId = 42;

        _provider
            .Setup(p => p.GetRepository(LearningType.Apprenticeship))
            .Returns(_repo.Object);

        var apprenticeshipLearningFactory = new ApprenticeshipLearningFactory();

        var expected = apprenticeshipLearningFactory.CreateNew(1, Guid.NewGuid());

        _repo
            .Setup(r => r.GetUnapprovedLearning(uln, approvalsId))
            .ReturnsAsync(expected);

        // Act
        var result = await _service.GetUnapprovedLearning(
            uln,
            LearningType.Apprenticeship,
            approvalsId);

        // Assert
        Assert.That(result, Is.EqualTo(expected));
        _provider.Verify(p => p.GetRepository(LearningType.Apprenticeship), Times.Once);
        _repo.Verify(r => r.GetUnapprovedLearning(uln, approvalsId), Times.Once);
    }

    [Test]
    public async Task AddLearning_UsesRepositoryBasedOnModelType()
    {
        // Arrange
        var apprenticeshipLearningFactory = new ApprenticeshipLearningFactory();
        var model = apprenticeshipLearningFactory.CreateNew(1, Guid.NewGuid());

        _provider
            .Setup(p => p.GetRepository(model))
            .Returns(_repo.Object);

        // Act
        await _service.AddLearning(model);

        // Assert
        _provider.Verify(p => p.GetRepository(model), Times.Once);
        _repo.Verify(r => r.AddLearning(model), Times.Once);
    }

    [Test]
    public async Task UpdateLearning_UsesRepositoryBasedOnModelType()
    {
        // Arrange
        var apprenticeshipLearningFactory = new ApprenticeshipLearningFactory();
        var model = apprenticeshipLearningFactory.CreateNew(1, Guid.NewGuid());

        _provider
            .Setup(p => p.GetRepository(model))
            .Returns(_repo.Object);

        // Act
        await _service.UpdateLearning(model);

        // Assert
        _provider.Verify(p => p.GetRepository(model), Times.Once);
        _repo.Verify(r => r.UpdateLearning(model), Times.Once);
    }
}