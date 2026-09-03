using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using SFA.DAS.Learning.Infrastructure.Configuration;

namespace SFA.DAS.Learning.InnerApi.Extensions;

/// <summary>
/// HealthCheckStartupExtensions
/// </summary>
public static class HealthCheckStartupExtensions
{
    private const string AzureResource = "https://database.windows.net/";
    
    // Take advantage of ChainedTokenCredential's built-in caching
    private static readonly ChainedTokenCredential AzureServiceTokenProvider = new(
        new ManagedIdentityCredential(),
        new AzureCliCredential(),
        new VisualStudioCodeCredential(),
        new VisualStudioCredential());
    
    /// <summary>
    /// Add health-checks
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="appSettings">Application Settings instance.</param>
    /// <returns></returns>
    public static IServiceCollection AddDasHealthChecks(this IServiceCollection services, ApplicationSettings appSettings)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(appSettings.DbConnectionString, configure: connection =>
            {
                {
                    var connectionStringBuilder = new SqlConnectionStringBuilder(appSettings.DbConnectionString);
                    var useManagedIdentity = !connectionStringBuilder.IntegratedSecurity && string.IsNullOrEmpty(connectionStringBuilder.UserID);

                    if (useManagedIdentity)
                    {
                        connection.AccessToken = AzureServiceTokenProvider.GetToken(new TokenRequestContext(scopes: [AzureResource])).Token;
                    }
                }
            });

        return services;
    }

    /// <summary>
    /// Wires up health checks to endpoints
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseDasHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/info", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = (context, _) =>
            {
                context.Response.ContentType = "application/json";
                var info = new
                {
                    Version = "1.0.0",
                    Name = "Apprenticeships Inner API"
                };
                return context.Response.WriteAsync(JsonSerializer.Serialize(info));
            }
        });

        return app.UseHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = (httpContext, report) => httpContext.Response.WriteJsonAsync(new
            {
                report.Status,
                report.TotalDuration,
                Results = report.Entries.ToDictionary(
                    e => e.Key,
                    e => new
                    {
                        e.Value.Status,
                        e.Value.Duration,
                        e.Value.Description,
                        e.Value.Data
                    })
            })
        });
    }
}