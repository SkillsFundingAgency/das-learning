using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace SFA.DAS.Learning.InnerApi.Identity.Authorization;

[ExcludeFromCodeCoverage]
public static class AuthorizationExtensions
{
	public static IServiceCollection AddApiAuthorization(this IServiceCollection services, bool isDevelopment = false)
	{

        if (isDevelopment)
        {
            services.AddAuthorization(x =>
            {
                {
                    x.AddPolicy("default", policy =>policy.AllowAnonymousUser());
                    x.DefaultPolicy = x.GetPolicy("default")!;
                }
            });
            services.AddSingleton<IAuthorizationHandler, LocalAuthorizationHandler>();
        }
        else
        {
            services.AddAuthorization(x =>
            {
                {
                    x.AddPolicy("default", policy => policy.RequireAuthenticatedUser());
                    x.DefaultPolicy = x.GetPolicy("default")!;
                }
            });
        }

        return services;
	}
}
