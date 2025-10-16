using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient.Standards;

namespace SFA.DAS.Learning.Infrastructure.Extensions;

public static class GetStandardFundingResponseExtensions
{
    public static bool IsApplicableForDate(this GetStandardFundingResponse record, DateTime startDate)
    {
        return record.EffectiveFrom <= startDate && (record.EffectiveTo == null || startDate <= record.EffectiveTo);
    }
}