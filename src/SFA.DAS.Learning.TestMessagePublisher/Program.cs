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
        services.ConfigureNServiceBusForSend(fullyQualifiedNamespace!);
    })
    .Build();

while (true)
{
    Console.WriteLine("Select an option...");
    Console.WriteLine("1. ApprenticeshipCreatedEvent");
    Console.WriteLine("X. Exit");
    var choice = Console.ReadLine();
    if (choice?.ToLower() == "x") break;

    if (choice == "1")
    {
        await PublishCreatedEvent(host.Services);
    }
}

async Task PublishCreatedEvent(IServiceProvider services)
{
    using IServiceScope serviceScope = services.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;
    var messagePublisher = provider.GetRequiredService<IMessageSession>();

    var firstNames = new[] { "Oliver", "George", "Harry", "Noah", "Jack", "Charlie", "Leo", "Jacob", "Freddie", "Alfie", "Archie", "Theo", "Thomas", "Arthur", "Oscar", "William", "Henry", "Joshua", "James", "Finley", "Max", "Lucas", "Isaac", "Harrison", "Teddy", "Alexander", "Edward", "Rowan", "Daniel", "Joseph", "Samuel", "Sebastian", "Tommy", "Riley", "Dylan", "David", "Benjamin", "Logan", "Jude", "Arlo", "Carter", "Toby", "Jaxon", "Luke", "Elliot", "Matthew", "Adam", "Elijah", "Jackson", "Liam" };
    var lastNames = new[] { "Smith", "Jones", "Taylor", "Williams", "Brown", "Davies", "Evans", "Wilson", "Thomas", "Roberts", "Johnson", "Lewis", "Walker", "Robinson", "Wood", "Thompson", "White", "Watson", "Jackson", "Wright", "Green", "Harris", "Cooper", "King", "Lee", "Martin", "Clarke", "James", "Morgan", "Hughes", "Edwards", "Hill", "Moore", "Clark", "Harrison", "Scott", "Young", "Morris", "Hall", "Ward", "Turner", "Carter", "Phillips", "Mitchell", "Patel", "Adams", "Campbell", "Anderson", "Allen", "Cook" };

    var random = new Random();
    var ukprn = 10005088;
    var employerId = 898989;

    var firstName = firstNames[random.Next(firstNames.Length)];
    var lastName = lastNames[random.Next(lastNames.Length)];

    var createdEvent = new ApprenticeshipCreatedEvent 
    {
        TransferSenderId = null, 
        Uln = random.Next(1000, 999999).ToString(), 
        ApprenticeshipId = random.Next(1000, 999999), 
        ProviderId = ukprn, 
        ActualStartDate = new DateTime(2025, 08, 01), 
        StartDate = new DateTime(2025, 08, 01),
        DateOfBirth = new DateTime(2000, 1, 1),
        AgreedOn = new DateTime(2025, 08, 01),
        CreatedOn = new DateTime(2025, 08, 01),
        AccountId = employerId, 
        LegalEntityName = "Local Test Message Publishing Ltd", 
        FirstName = firstName,
        LastName = lastName,
        IsOnFlexiPaymentPilot = true,
        PriceEpisodes = new[] 
        { 
            new PriceEpisode 
            { 
                Cost = 10000m,
                FromDate = new DateTime(2025, 08, 01),
                ToDate = new DateTime(2026, 07, 31),
                TrainingPrice = 9000m,
                EndPointAssessmentPrice = 1000m
            } 
        }, 
        EndDate = new DateTime(2026, 07, 31), 
        TrainingCode = "123"
    };

    await messagePublisher.Publish(createdEvent);

    var putRequest = new 
    {
        learner = new 
        {
            firstName = createdEvent.FirstName,
            lastName = createdEvent.LastName,
            email = $"{createdEvent.FirstName}.{createdEvent.LastName}@example.com",
            dob = createdEvent.DateOfBirth,
            hasEhcp = false,
            uln = long.Parse(createdEvent.Uln)
        },
        delivery = new 
        {
            onProgramme = new[] 
            {
                new 
                {
                    standardCode = int.Parse(createdEvent.TrainingCode),
                    agreementId = "AGREEMENT1",
                    startDate = createdEvent.StartDate,
                    expectedEndDate = createdEvent.EndDate,
                    costs = createdEvent.PriceEpisodes.Select(p => new 
                    {
                        trainingPrice = (int?)p.TrainingPrice,
                        epaoPrice = (int?)p.EndPointAssessmentPrice,
                        fromDate = p.FromDate
                    }).ToList(),
                    completionDate = (DateTime?)null,
                    achievementDate = (DateTime?)null,
                    withdrawalDate = (DateTime?)null,
                    pauseDate = (DateTime?)null,
                    learningSupport = new List<object>(),
                    care = new 
                    {
                        careleaver = false,
                        employerConsent = true
                    },
                    aimSequenceNumber = 1,
                    learnAimRef = "ZPROG001"
                }
            },
            englishAndMaths = new List<object>()
        }
    };

    var innerPutRequest = new
    {
        Delivery = new { WithdrawalDate = (DateTime?)null },
        Learner = new
        {
            FirstName = createdEvent.FirstName,
            LastName = createdEvent.LastName,
            EmailAddress = $"{createdEvent.FirstName}.{createdEvent.LastName}@example.com",
            DateOfBirth = createdEvent.DateOfBirth,
            Care = new
            {
                HasEHCP = false,
                IsCareLeaver = false,
                CareLeaverEmployerConsentGiven = true
            },
            CompletionDate = (DateTime?)null,
            AchievementDate = (DateTime?)null
        },
        OnProgramme = new
        {
            ExpectedEndDate = createdEvent.EndDate,
            Costs = createdEvent.PriceEpisodes.Select(p => new
            {
                TrainingPrice = (int)p.TrainingPrice,
                EpaoPrice = (int?)p.EndPointAssessmentPrice,
                FromDate = p.FromDate
            }).ToList(),
            PauseDate = (DateTime?)null,
            BreaksInLearning = new List<object>()
        },
        EnglishAndMathsCourses = new List<object>(),
        LearningSupport = new List<object>()
    };

    var outerFileName = $"{createdEvent.ProviderId}-{createdEvent.FirstName}-{createdEvent.LastName}-{createdEvent.Uln}-Outer.json";
    var outerJson = System.Text.Json.JsonSerializer.Serialize(putRequest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(outerFileName, outerJson);

    var innerFileName = $"{createdEvent.ProviderId}-{createdEvent.FirstName}-{createdEvent.LastName}-{createdEvent.Uln}-Inner.json";
    var innerJson = System.Text.Json.JsonSerializer.Serialize(innerPutRequest, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    await File.WriteAllTextAsync(innerFileName, innerJson);

    Console.WriteLine($"First Name: {firstName}");
    Console.WriteLine($"Last Name: {lastName}");
    Console.WriteLine($"UKPRN: {ukprn}");
    Console.WriteLine($"ULN: {createdEvent.Uln}");
    Console.WriteLine($"Message published");
    Console.WriteLine($"Outer PUT request saved to {outerFileName}");
    Console.WriteLine($"Inner PUT request saved to {innerFileName}");
    Console.WriteLine();
}
static void ForceAssemblyLoad()
{
    var apprenticeshipEarningsTypes = new ApprenticeshipCreatedEvent();
}