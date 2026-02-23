using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.CreateDraftShortCourse;

[TestFixture]
public class WhenCreateDraftShortCourseCommandIsHandled
{
    private CreateDraftShortCourseCommandHandler _commandHandler = null!;
    private Mock<ILearnerFactory> _learnerFactory = null!;
    private Mock<ILearnerRepository> _learnerRepository = null!;
    private Mock<IShortCourseLearningFactory> _learningFactory = null!;
    private Mock<IShortCourseLearningRepository> _learningRepository = null!;
    private Mock<ILogger<CreateDraftShortCourseCommandHandler>> _logger = null!;
    private Fixture _fixture = null!;

    [SetUp]
    public void SetUp()
    {
        _learnerFactory = new Mock<ILearnerFactory>();
        _learnerRepository = new Mock<ILearnerRepository>();
        _learningFactory = new Mock<IShortCourseLearningFactory>();
        _learningRepository = new Mock<IShortCourseLearningRepository>();
        _logger = new Mock<ILogger<CreateDraftShortCourseCommandHandler>>();

        _commandHandler = new CreateDraftShortCourseCommandHandler(
            _learnerFactory.Object,
            _learnerRepository.Object,
            _learningRepository.Object,
            _learningFactory.Object,
            _logger.Object);

        _fixture = new Fixture();
    }

    [Test]
    public async Task ThenANewShortCourseLearningIsCreated()
    {
        // Arrange
        var command = _fixture.Create<CreateDraftShortCourseCommand>();

        var learnerDomainModel = _fixture.Create<LearnerDomainModel>();
        var domainModel = _fixture.Create<ShortCourseLearningDomainModel>();

        _learnerFactory.Setup(x => x.CreateNew(It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<string>(),It.IsAny<string>(), It.IsAny<string?>())).Returns(learnerDomainModel);
        _learningFactory.Setup(x => x.CreateNew(It.IsAny<Guid>(), command.Model.OnProgramme.CompletionDate)).Returns(domainModel);

        // Act
        var result = await _commandHandler.Handle(command);

        // Assert
        _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
        result.LearningKey.Should().Be(domainModel.Key);
    }
}
