namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class StringExtensions
{
    internal static string? GetPropertyValue(this string input, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(propertyName))
            return null;

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in tokens)
        {
            var parts = token.Split(':', 2);
            if (parts.Length == 2 && parts[0].Equals(propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }
        }

        return null;
    }
}
