using SFA.DAS.Learning.Command;

namespace SFA.DAS.Learning.InnerApi.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            logger.LogWarning(ex, "Resource not found: {Message}", ex.Message);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
