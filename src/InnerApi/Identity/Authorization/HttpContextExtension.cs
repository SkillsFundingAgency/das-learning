namespace SFA.DAS.Learning.InnerApi.Identity.Authorization;

/// <summary>
/// Authorization extension for HttpContext
/// </summary>
public static class HttpContextExtension
{
    /// <summary>
    /// Get the user id from the HttpContext
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns>User Id</returns>
    /// <exception cref="UnauthorizedAccessException">Throws exception if userId not found</exception>
    public static string GetUserId(this HttpContext httpContext)
    {
        if(!httpContext.Items.TryGetValue("UserId", out var userId) || userId == null)
            throw new UnauthorizedAccessException("User Id not found in HttpContext");

        var userIdAsString = userId as string;

        if(string.IsNullOrEmpty(userIdAsString))
            throw new UnauthorizedAccessException("User Id Key exists in HttpContext, but the value is empty");

        return (string)userId;

    }
}
