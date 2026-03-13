using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using SFA.DAS.Learning.Command.Decorators;
using SFA.DAS.Learning.Domain.Factories;
using SFA.DAS.Learning.Domain.Repositories;
using SFA.DAS.Learning.Infrastructure.LearningOuterApiClient;
using SFA.DAS.Learning.Infrastructure.Services;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Learning.Command.ArchiveLearningHistory;
using SFA.DAS.Learning.Domain.Services;

namespace SFA.DAS.Learning.Command;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection
            .AddCommandHandlers(AddCommandHandlerDecorators)
            .AddScoped<ICommandDispatcher, CommandDispatcher>()
            .AddScoped<ILearnerFactory, LearnerFactory>()
            .AddScoped<IApprenticeshipLearningFactory, ApprenticeshipLearningFactory>()
            .AddScoped<IShortCourseLearningFactory, ShortCourseLearningFactory>()
            .AddScoped<ILearningHistoryRepository, LearningHistoryRepository>()
            .AddSingleton<ISystemClockService, SystemClockService>()
            .AddEncodingServices(configuration)
            .AddPersistenceServices();

        return serviceCollection;
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection, Func<IServiceCollection, IServiceCollection> addDecorators = null)
    {
        serviceCollection.Scan(scan =>
        {
            scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime();

            scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime();
        });

        serviceCollection.AddScoped<IArchiveLearningHistoryCommandHandler, ArchiveLearningHistoryCommandHandler>();

        if (addDecorators != null)
        {
            serviceCollection = addDecorators(serviceCollection);
        }

        return serviceCollection;
    }

    private static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        serviceCollection.AddScoped<ILearnerRepository, LearnerRepository>();
        serviceCollection.AddScoped<IApprenticeshipLearningRepository, ApprenticeshipLearningRepository>();
        serviceCollection.AddScoped<IShortCourseLearningRepository, ShortCourseLearningRepository>();
        serviceCollection.AddScoped<ILearningService, LearningService>();
        serviceCollection.AddScoped<ILearningRepositoryProvider, LearningRepositoryProvider>();
        return serviceCollection;
    }

    private static IServiceCollection AddEncodingServices(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var encodingConfigJson = configuration.GetSection("SFA.DAS.Encoding").Value;
        var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
        serviceCollection.AddSingleton(encodingConfig);
        serviceCollection.AddSingleton<IEncodingService, EncodingService>();
        return serviceCollection;
    }

    private static IServiceCollection AddCommandHandlerDecorators(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .Decorate(typeof(ICommandHandler<>), typeof(CommandHandlerWithUnitOfWork<>));

        return serviceCollection;
    }

    public static IServiceCollection AddLearningOuterApiClient(this IServiceCollection serviceCollection, string baseAddress, string key)
    {
        baseAddress = EnsureBaseAddressFormat(baseAddress);
        serviceCollection.AddScoped<ILearningOuterApiClient, LearningOuterApiClient>(x =>
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
            httpClient.DefaultRequestHeaders.Add("X-Version", "1");
            return new LearningOuterApiClient(httpClient);
        });

        return serviceCollection;
    }

    private static string EnsureBaseAddressFormat(string baseAddress)
    {
        if (baseAddress.EndsWith('/'))
            return baseAddress;
            
        return baseAddress + '/';
    }
}