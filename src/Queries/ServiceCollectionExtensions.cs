using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Learning.Queries.GetLearnings;

namespace SFA.DAS.Learning.Queries;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQueryServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .Scan(scan =>
            {
                scan.FromExecutingAssembly()
                    .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();

                scan.FromAssembliesOf(typeof(GetLearningsQueryHandler))
                    .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            })
            .AddScoped<IQueryDispatcher, QueryDispatcher>();

        return serviceCollection;
    }
}