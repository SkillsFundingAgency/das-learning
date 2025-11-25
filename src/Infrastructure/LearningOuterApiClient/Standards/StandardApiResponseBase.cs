using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

#pragma warning disable CS8618
[ExcludeFromCodeCoverage]
public abstract class StandardApiResponseBase
{
    public StandardDate StandardDates { get; set; }
    public List<ApprenticeshipFunding> ApprenticeshipFunding { get; set; }

    [JsonIgnore]
    public int MaxFunding => GetFundingDetails(nameof(MaxFunding));
    [JsonIgnore]
    public int TypicalDuration => GetFundingDetails(nameof(TypicalDuration));
    [JsonIgnore]
    public bool IsActive => this.IsStandardActive();
    public int MaxFundingOn(DateTime effectiveDate) => GetFundingDetails(nameof(MaxFunding), effectiveDate);
    protected virtual int GetFundingDetails(string prop, DateTime? effectiveDate = null) => this.FundingDetails(prop, effectiveDate);
}
#pragma warning restore CS8618