using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Learning.Command.CreateDraftShortCourse;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using System.Threading.Tasks;
using FluentAssertions;
using SFA.DAS.Learning.Domain.Apprenticeship;

namespace SFA.DAS.Learning.Command.UnitTests.CreateDraftShortCourse
{
    [TestFixture]
    public class WhenCreateDraftShortCourseCommandIsHandled
    {
        private CreateDraftShortCourseCommandHandler _commandHandler = null!;
        private Mock<IShortCourseLearningFactory> _learningFactory = null!;
        private Mock<IShortCourseLearningRepository> _learningRepository = null!;
        private Mock<IMessageSession> _messageSession = null!;
        private Mock<ILogger<CreateDraftShortCourseCommandHandler>> _logger = null!;
        private Fixture _fixture = null!;

        [SetUp]
        public void SetUp()
        {
            _learningFactory = new Mock<IShortCourseLearningFactory>();
            _learningRepository = new Mock<IShortCourseLearningRepository>();
            _messageSession = new Mock<IMessageSession>();
            _logger = new Mock<ILogger<CreateDraftShortCourseCommandHandler>>();

            _commandHandler = new CreateDraftShortCourseCommandHandler(
                _learningRepository.Object,
                _learningFactory.Object,
                _messageSession.Object,
                _logger.Object);

            _fixture = new Fixture();
        }

        [Test]
        public async Task ThenANewShortCourseLearningIsCreated()
        {
            // Arrange
            var command = _fixture.Create<CreateDraftShortCourseCommand>();

            var domainModel = _fixture.Create<ShortCourseLearningDomainModel>();

            _learningFactory.Setup(x => x.CreateNew(
                    command.Model.OnProgramme.WithdrawalDate,
                    command.Model.OnProgramme.ExpectedEndDate,
                    command.Model.OnProgramme.CompletionDate,
                    false))
                .Returns(domainModel);

            // Act
            var result = await _commandHandler.Handle(command);

            // Assert
            _learningRepository.Verify(x => x.Add(It.Is<ShortCourseLearningDomainModel>(y => y == domainModel)));
            result.LearningKey.Should().Be(domainModel.Key);
        }
    }
}
