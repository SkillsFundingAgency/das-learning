using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Learning.Infrastructure.Configuration;

var builder = new ConfigurationBuilder()
    .AddAzureTableStorage(options =>
    {
        options.ConfigurationKeys = new[] { "SFA.DAS.Learning" };
        options.StorageConnectionString = "UseDevelopmentStorage=true;";
        options.EnvironmentName = "LOCAL";
        options.PreFixConfigurationKeys = false;
    });

var config = builder.Build();
var settings = new ApplicationSettings();
config.Bind(nameof(ApplicationSettings), settings);

Console.WriteLine($"NServiceBusConnectionString: {settings.NServiceBusConnectionString}");
