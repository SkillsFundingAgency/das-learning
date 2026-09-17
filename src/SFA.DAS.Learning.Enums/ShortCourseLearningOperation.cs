using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShortCourseLearningOperation
{
    Created = 0,
    Updated = 1,
    Removed = 2,
    Approved = 3
}
