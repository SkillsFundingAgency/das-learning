using Newtonsoft.Json;

namespace SFA.DAS.Learning.AcceptanceTests.Helpers;

internal static class JsonConverterExtensions
{

    internal static T? DeserializeOrDefault<T>(this string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
        catch
        {
            return default(T);
        }
    }
}
