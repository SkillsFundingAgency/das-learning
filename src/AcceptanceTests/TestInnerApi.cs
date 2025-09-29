using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SFA.DAS.Learning.AcceptanceTests.Helpers;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.InnerApi.Services;
using SFA.DAS.Learning.Queries;

namespace SFA.DAS.Learning.AcceptanceTests;

public class TestInnerApi : IDisposable
{
    private readonly TestContext _testContext;
    private readonly TestServer _testServer;
    private readonly HttpClient _httpClient;
    private readonly IEnumerable<MessageHandler> _queueTriggeredFunctions;
    private bool _isDisposed;


    public TestInnerApi(TestContext testContext)
    {
        _testContext = testContext;

        var builder = new WebHostBuilder()
            .UseEnvironment("SystemAcceptanceTests")
            .ConfigureServices(services =>
            {
                services.AddControllers()
                    .AddApplicationPart(typeof(SFA.DAS.Learning.InnerApi.Controllers.LearningController).Assembly);

                services.AddQueryServices().AddCommandServices(_testContext.GenerateConfiguration()).AddEventServices();
                services.AddSingleton<IMessageSession>(_testContext.MessageSession);
                services.AddScoped<IPagedLinkHeaderService, PagedLinkHeaderService>();

                AddEntityFrameworkForApprenticeships(services, testContext.SqlDatabase?.DatabaseInfo.ConnectionString!);
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            });

        _testServer = new TestServer(builder);
        _httpClient = _testServer.CreateClient();

    }

    public static IServiceCollection AddEntityFrameworkForApprenticeships(IServiceCollection services, string connectionString)
    {
        services.AddScoped(p =>
        {
            var options = new DbContextOptionsBuilder<LearningDataContext>()
                .UseSqlServer(new SqlConnection(connectionString), optionsBuilder => optionsBuilder.CommandTimeout(7200)) //7200=2hours
                .Options;
            return new LearningDataContext(options);
        });

        return services.AddScoped(provider =>
        {
            var dataContext = provider.GetService<LearningDataContext>() ?? throw new ArgumentNullException("LearningDataContext");
            return new Lazy<LearningDataContext>(dataContext);
        });
    }


    public async Task Patch<T>(string route, T body)
    {
        var content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PatchAsync(route, content);
    }

    public async Task Post<T>(string route, T body)
    {
        var content = new StringContent(JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(route, content);
    }

    internal async Task Delete(string route)
    {
        var response = await _httpClient.DeleteAsync(route);

        if(response.IsSuccessStatusCode) return;
        Console.WriteLine($"Response: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
    }

    public async Task PublishEvent<T>(T eventObject)
    {
        var response = await _httpClient.GetAsync("/home");
        var responseString = await response.Content.ReadAsStringAsync();
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
            _testServer.Dispose();
            _httpClient.Dispose();
        }

        _isDisposed = true;
    }
}
