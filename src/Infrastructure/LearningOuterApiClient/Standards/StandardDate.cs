using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

[ExcludeFromCodeCoverage]
public class StandardDate
{
    public DateTime? LastDateStarts { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public DateTime EffectiveFrom { get; set; }
}