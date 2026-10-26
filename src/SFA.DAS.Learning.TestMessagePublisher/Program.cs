using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Infrastructure.Extensions;

ForceAssemblyLoad();

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("local.settings.json")
    .AddEnvironmentVariables()
    .Build();

var fullyQualifiedNamespace = config["fullyQualifiedNamespace"];

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.ConfigureNServiceBusForSend(fullyQualifiedNamespace!, false);
    })
    .Build();

Console.WriteLine("Select an option...");
Console.WriteLine("1. ApprovalCreatedEvent");
Console.WriteLine("X. Exit");
var choice = Console.ReadLine();
if (choice == "1")
{
    await PublishCreatedEvent(host.Services);
}

async Task PublishCreatedEvent(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    var messagePublisher = provider.GetRequiredService<IMessageSession>();

    await messagePublisher.Publish(new ApprenticeshipCreatedEvent {TransferSenderId = 123436, Uln = "2135546", ApprenticeshipId = 34254, ProviderId = 4536546, ActualStartDate = DateTime.Now, AccountId = 2344536, LegalEntityName = "Test", PriceEpisodes = new[] { new PriceEpisode { Cost = 122345m } }, EndDate = DateTime.Now.AddYears(1), TrainingCode = "ABC123"});

    Console.WriteLine("Message published.");
    Console.WriteLine("Press enter to quit");
    Console.ReadLine();
}
static void ForceAssemblyLoad()
{
    var apprenticeshipEarningsTypes = new ApprenticeshipCreatedEvent();
}