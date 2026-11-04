using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Learning.Command.ClearFurtherApprovalNeeded;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Domain.Services;
using SFA.DAS.Learning.Enums;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Learning.Command.UnitTests.ClearFurtherApprovalNeeded;

[TestFixture]
public class WhenAClearFurtherApprovalNeededCommandIsSent
{
    private ClearFurtherApprovalNeededCommandHandler _handler = null!;
    private Mock<ILearningService> _learningService = null!;
    private ClearFurtherApprovalNeededCommand _command = null!;

    [SetUp]
    public void SetUp()
    {
        _learningService = new Mock<ILearningService>();
        _handler = new ClearFurtherApprovalNeededCommandHandler(
            _learningService.Object,
            Mock.Of<ILogger<ClearFurtherApprovalNeededCommandHandler>>());

        _command = new ClearFurtherApprovalNeededCommand
        {
            LearningType = LearningType.Apprenticeship,
            LearningKey = Guid.NewGuid(),
            EpisodeKey = Guid.NewGuid()
        };
    }

    [Test]
    public async Task ThenTheFlagIsClearedOnTheEpisodeAndTheLearningIsUpdated()
    {
        var learning = new Mock<LearningDomainModel>();
        learning.Setup(x => x.ClearFurtherApprovalNeeded(_command.EpisodeKey)).Returns(true);
        _learningService.Setup(x => x.GetLearning(_command.LearningKey, _command.LearningType)).ReturnsAsync(learning.Object);

        await _handler.Handle(_command);

        learning.Verify(x => x.ClearFurtherApprovalNeeded(_command.EpisodeKey), Times.Once);
        _learningService.Verify(x => x.UpdateLearning(learning.Object), Times.Once);
    }

    [Test]
    public void AndTheLearningIsNotFoundThenNotFoundIsThrownAndNothingIsUpdated()
    {
        _learningService.Setup(x => x.GetLearning(_command.LearningKey, _command.LearningType)).ReturnsAsync((LearningDomainModel?)null);

        Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_command));

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }

    [Test]
    public void AndTheEpisodeIsNotFoundThenNotFoundIsThrownAndNothingIsUpdated()
    {
        var learning = new Mock<LearningDomainModel>();
        learning.Setup(x => x.ClearFurtherApprovalNeeded(_command.EpisodeKey)).Returns(false);
        _learningService.Setup(x => x.GetLearning(_command.LearningKey, _command.LearningType)).ReturnsAsync(learning.Object);

        Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(_command));

        _learningService.Verify(x => x.UpdateLearning(It.IsAny<LearningDomainModel>()), Times.Never);
    }
}
