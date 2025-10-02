using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.DataAccess;
using SFA.DAS.Learning.Domain;
using SFA.DAS.Learning.Functions.AppStart;
using SFA.DAS.Learning.Infrastructure.Configuration;
using SFA.DAS.Learning.Infrastructure.Extensions;

namespace SFA.DAS.Learning.Functions;

[ExcludeFromCodeCoverage]
public class Startup
{
    public IConfiguration Configuration { get; set; }

    private ApplicationSettings _applicationSettings;
    public ApplicationSettings ApplicationSettings
    {
        get
        {
            if (_applicationSettings == null)
            {
                _applicationSettings = new ApplicationSettings();
                Configuration.Bind(nameof(ApplicationSettings), _applicationSettings);
            }
            return _applicationSettings;
        }
    }

    public Startup()
    {
    }

    public void Configure(IHostBuilder builder)
    {
        builder
            .ConfigureAppConfiguration(PopulateConfig)
            .ConfigureNServiceBusForSubscribe()
            .ConfigureServices((c, s) =>
            {
                SetupServices(s);
                s.ConfigureNServiceBusForSend(ApplicationSettings.NServiceBusConnectionString.GetFullyQualifiedNamespace());
            });
    }

    private void PopulateConfig(IConfigurationBuilder configurationBuilder)
    {
        Environment.SetEnvironmentVariable("ENDPOINT_NAME", Constants.EndpointName);

        configurationBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("local.settings.json", true);

        var configuration = configurationBuilder.Build();
        configurationBuilder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
            options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            options.EnvironmentName = configuration["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
            options.ConfigurationKeysRawJsonResult = new[] { "SFA.DAS.Encoding" };
        });

        Configuration = configurationBuilder.Build();
    }

    public void SetupServices(IServiceCollection services)
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));
        services.AddOptions();

        services.AddEntityFrameworkForApprenticeships(ApplicationSettings, NotLocal(Configuration));

        services.AddCommandServices(Configuration).AddEventServices();

        if (NotAcceptanceTests(Configuration))
            services.AddApprenticeshipsOuterApiClient(ApplicationSettings.ApprenticeshipsOuterApiConfiguration.BaseUrl, ApplicationSettings.ApprenticeshipsOuterApiConfiguration.Key);

        services.AddLogging((options) =>
        {
            options.AddApplicationInsights();
            options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by default
            options.SetMinimumLevel(LogLevel.Information);
        });

        services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();
    }


    private static bool NotAcceptanceTests(IConfiguration configuration)
    {
        return !configuration!["EnvironmentName"].Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
    }

    private static bool NotLocal(IConfiguration configuration)
    {
        var env = configuration!["EnvironmentName"];
        var isLocal = env.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase);
        var isLocalAcceptanceTests = env.Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        return !isLocal && !isLocalAcceptanceTests;
    }
}