using Microsoft.Extensions.DependencyInjection;
using Moq;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Functions;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;

namespace SFA.DAS.Learning.AcceptanceTests;

internal class TestFunctionStartup
{
    private readonly Startup _startUp;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private readonly IMessageSession _messageSession;
    private readonly Mock<IApprenticeshipsOuterApiClient> _mockApprenticeshipsOuterApiClient;

    public TestFunctionStartup(
        TestContext testContext,
        IEnumerable<MessageHandler> queueTriggeredFunctions,
        IMessageSession messageSession,
        Mock<IApprenticeshipsOuterApiClient> mockApprenticeshipsOuterApiClient)
    {
        _startUp = new Startup();
        _startUp.Configuration = testContext.GenerateConfiguration();
        _queueTriggeredFunctions = queueTriggeredFunctions;
        _messageSession = messageSession;
        _mockApprenticeshipsOuterApiClient = mockApprenticeshipsOuterApiClient;
    }

    public void Configure()
    {
        // Intentionally left blank
    }

    public void ConfigureServices(IServiceCollection collection)
    {
        _startUp.SetupServices(collection);

        collection.AddSingleton<IMessageSession>(_messageSession);

        foreach (var queueTriggeredFunction in _queueTriggeredFunctions)
        {
            collection.AddTransient(queueTriggeredFunction.HandlerType);
        }

        collection.AddScoped<IApprenticeshipsOuterApiClient>(_ => _mockApprenticeshipsOuterApiClient.Object);
    }
}