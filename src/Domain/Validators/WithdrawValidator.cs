using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Infrastructure.Services;

namespace SFA.DAS.Learning.Domain.Validators;

#pragma warning disable 8618
public class WithdrawDomainRequest
{
    public long UKPRN { get; set; }
    public string ULN { get; set; }
    public string Reason { get; set; }
    public string ReasonText { get; set; }
    public DateTime LastDayOfLearning { get; set; }
    public string ProviderApprovedBy { get; set; }
}
#pragma warning restore 8618

public enum WithdrawReason
{
    WithdrawFromStart,
    WithdrawDuringLearning,
    WithdrawFromBeta,
    Other
}

public class WithdrawValidator : IValidator<WithdrawDomainRequest>
{
    private readonly ISystemClockService _systemClockService;

    public WithdrawValidator(ISystemClockService systemClockService)
    {
        _systemClockService = systemClockService;
    }

    public bool IsValid(WithdrawDomainRequest request, out string message, params object?[] args)
    {
        message = string.Empty;

        var apprenticeship = args.OfType<LearningDomainModel>().FirstOrDefault();
        var currentAcademicYearEnd = args.OfType<DateTime>().FirstOrDefault();

        // Validate if apprenticeship exists
        if (apprenticeship == null)
            return FailWithMessage(out message, $"No apprenticeship found for ULN {request.ULN}");

        if (apprenticeship.LatestEpisode.Ukprn != request.UKPRN) // This check should really be part of authorization, but is currently passedd in as part of the request body
            return FailWithMessage(out message, $"Learning not found for ULN {request.ULN} and UKPRN {request.UKPRN}");

        // Validate if already withdrawn
        if (apprenticeship.LatestEpisode.LearningStatus == LearnerStatus.Withdrawn)
            return FailWithMessage(out message, $"Learning already withdrawn for ULN {request.ULN}");

        // Validate Reason
        if (!ValidateReason(request, out message))
            return false;

        // Validate Withdrawal Date
        if (!ValidateWithdrawalDate(request, apprenticeship, currentAcademicYearEnd, out message))
            return false;

        message = string.Empty;

        return true;
    }

    private bool ValidateReason(WithdrawDomainRequest request, out string message)
    {
        WithdrawReason reason;

        if (!Enum.TryParse(request.Reason, out reason))
        {
            var validReasons = string.Join(", ", Enum.GetNames(typeof(WithdrawReason)));
            return FailWithMessage(out message, $"Invalid reason, possible values are {validReasons}");
        }

        if (reason == WithdrawReason.Other && string.IsNullOrWhiteSpace(request.ReasonText))
            return FailWithMessage(out message, "Reason text is required for 'Other' reason");

        if (reason == WithdrawReason.Other && request.ReasonText.Length > 100)
            return FailWithMessage(out message, "Reason text must be less than 100 characters");

        message = string.Empty;
        return true;
    }

    private bool ValidateWithdrawalDate(WithdrawDomainRequest request, LearningDomainModel learning, DateTime currentAcademicYearEnd, out string message)
    {
        message = string.Empty;
        var now = _systemClockService.UtcNow;

        if (request.LastDayOfLearning < learning.StartDate)
            return FailWithMessage(out message, "LastDayOfLearning cannot be before the start date");

        if (request.LastDayOfLearning > currentAcademicYearEnd)
            return FailWithMessage(out message, "LastDayOfLearning cannot be after the end of the current academic year");

        message = string.Empty;
        return true;
    }

    private static bool FailWithMessage(out string message, string failReason)
    {
        message = failReason;
        return false;
    }
}