namespace SFA.DAS.Learning.Infrastructure.LearningOuterApiClient.Standards;

public static class StandardApiResponseBaseExtensions
{
    public static int FundingDetails(this StandardApiResponseBase standardApiResponseBase, string prop, DateTime? effectiveDate = null)
    {
        if (standardApiResponseBase.ApprenticeshipFunding == null || !standardApiResponseBase.ApprenticeshipFunding.Any()) return 0;

        var effDate = effectiveDate ?? DateTime.UtcNow;

        var funding = standardApiResponseBase.ApprenticeshipFunding
            .FirstOrDefault(c =>
                c.EffectiveFrom <= effDate
                && (c.EffectiveTo == null || c.EffectiveTo >= effDate));

        if (funding == null)
        {
            funding = standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault(c => c.EffectiveTo == null);
        }

        if (prop == nameof(standardApiResponseBase.MaxFunding))
        {
            return funding?.MaxEmployerLevyCap
                   ?? standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault()?.MaxEmployerLevyCap
                   ?? 0;
        }

        return funding?.Duration
               ?? standardApiResponseBase.ApprenticeshipFunding.FirstOrDefault()?.Duration
               ?? 0;
    }

    public static bool IsStandardActive(this StandardApiResponseBase standardApiResponseBase)
    {
        if (standardApiResponseBase.StandardDates == null) return false;

        return standardApiResponseBase.StandardDates.EffectiveFrom.Date <= DateTime.UtcNow.Date
               && (!standardApiResponseBase.StandardDates.EffectiveTo.HasValue ||
                   standardApiResponseBase.StandardDates.EffectiveTo.Value.Date >= DateTime.UtcNow.Date)
               && (!standardApiResponseBase.StandardDates.LastDateStarts.HasValue ||
                   standardApiResponseBase.StandardDates.LastDateStarts.Value.Date >= DateTime.UtcNow.Date);
    }
}