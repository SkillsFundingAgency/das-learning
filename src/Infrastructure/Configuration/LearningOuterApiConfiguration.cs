using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.Configuration;

#pragma warning disable CS8618
[ExcludeFromCodeCoverage]
public class LearningOuterApiConfiguration
{
    public string Key { get; set; }
    public string BaseUrl { get; set; }
}
#pragma warning restore CS8618