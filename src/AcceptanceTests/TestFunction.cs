using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Moq;
using NServiceBus.Testing;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Functions;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

namespace SFA.DAS.Learning.AcceptanceTests;

public class TestFunction : IDisposable
{
    private readonly TestContext _testContext;
    private readonly TestServer _testServer;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private bool _isDisposed;

    public string HubName { get; }

    public Mock<ILearningOuterApiClient> mockLearningOuterApiClient  { get; }

    public TestFunction(TestContext testContext, string hubName)
    {

        HubName = hubName;
        _testContext = testContext;
        var _ = new Startup();// This forces the AzureFunction assembly to load
        _queueTriggeredFunctions = MessageHandlerHelper.GetMessageHandlers();
        mockLearningOuterApiClient = GetMockOuterApi();

        _testServer = new TestServer(new WebHostBuilder()
        .UseEnvironment(Environments.Development)
            .UseStartup<TestFunctionStartup>((_) => new TestFunctionStartup(testContext, _queueTriggeredFunctions, _testContext.MessageSession, mockLearningOuterApiClient)));

    }

    public async Task PublishEvent<T>(T eventObject)
    {
        var function = _queueTriggeredFunctions.FirstOrDefault(x => x.HandledEventType == typeof(T));
        var handler = _testServer.Services.GetService(function.HandlerType) as IHandleMessages<T>;
        var context = new TestableMessageHandlerContext
        {
            CancellationToken = new CancellationToken()
        };

        try
        {
            await handler.Handle(eventObject, context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred in {handler.GetType().Name}", ex);// Some of the tests verify the behaviour on handler errors, for this reason the exception is swallowed
        }
    }

    public async Task DisposeAsync()
    {
        Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            // no components to dispose
        }

        _isDisposed = true;
    }

    private static Mock<ILearningOuterApiClient> GetMockOuterApi()
    {
        var mockApprenticeshipsOuterApiClient = new Mock<ILearningOuterApiClient>();

        mockApprenticeshipsOuterApiClient.Setup(x => x.GetStandard(It.IsAny<int>())).ReturnsAsync(new GetStandardResponse
        {
            MaxFunding = int.MaxValue,
            ApprenticeshipFunding = new List<GetStandardFundingResponse>
            {
            new GetStandardFundingResponse{ EffectiveFrom = DateTime.MinValue, EffectiveTo = null, MaxEmployerLevyCap = int.MaxValue }
            }
        });

        return mockApprenticeshipsOuterApiClient;
    }
}