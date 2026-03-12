using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ShortCourseUpdateChanges
{
    WithdrawalDate = 0,
    Milestone = 1,
    CompletionDate = 2
}
