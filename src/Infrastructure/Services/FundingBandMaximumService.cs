using Microsoft.Extensions.Logging;
using SFA.DAS.Learning.Infrastructure.ApprenticeshipsOuterApiClient;
using SFA.DAS.Learning.Infrastructure.Extensions;

namespace SFA.DAS.Learning.Infrastructure.Services;

public class FundingBandMaximumService : IFundingBandMaximumService
{
    private readonly IApprenticeshipsOuterApiClient _apprenticeshipsOuterApiClient;
    private readonly ILogger<FundingBandMaximumService> _logger;

    public FundingBandMaximumService(IApprenticeshipsOuterApiClient apprenticeshipsOuterApiClient, ILogger<FundingBandMaximumService> logger)
    {
        _apprenticeshipsOuterApiClient = apprenticeshipsOuterApiClient;
        _logger = logger;
    }

    public async Task<int?> GetFundingBandMaximum(int courseCode, DateTime? startDate)
    {
        var standard = await _apprenticeshipsOuterApiClient.GetStandard(courseCode);

        if (startDate == null)
            return null;

        return standard.ApprenticeshipFunding
            .SingleOrDefault(x => x.IsApplicableForDate(startDate.Value))?.MaxEmployerLevyCap;
    }

    public async Task<int?> GetNextApplicableFundingBandMaximum(int courseCode, DateTime startDate)
    {
        var standard = await _apprenticeshipsOuterApiClient.GetStandard(courseCode);

        // if a record exists for the start date simply return it
        var applicableRecord = standard.ApprenticeshipFunding
            .SingleOrDefault(x => x.IsApplicableForDate(startDate));

        if (applicableRecord != null)
            return applicableRecord.MaxEmployerLevyCap;

        // if not find the next effective record for that course
        var nextRecord = standard.ApprenticeshipFunding
            .Where(x => x.EffectiveFrom > startDate)
            .OrderBy(x => x.EffectiveFrom)
            .FirstOrDefault();

        // use it only if it starts within the same month
        if (nextRecord != null &&
            nextRecord.EffectiveFrom.Year == startDate.Year &&
            nextRecord.EffectiveFrom.Month == startDate.Month)
        {
            return nextRecord.MaxEmployerLevyCap;
        }

        // otherwise, no applicable record exists (this should have been caught by ILR validation)
        return null;
    }

    
}