using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.DataAccess.Extensions;

public static class LinqExtensions
{
    public static IQueryable<ApprenticeshipLearning> IsActiveInYear(
        this IQueryable<ApprenticeshipLearning> source, DateTime startOfAcademicYear, DateTime endOfAcademicYear)
    {
        return source
                .Where(x =>
                    // Exclude if Completed before start of activeOnDate year
                    !(x.CompletionDate.HasValue && x.CompletionDate.Value < startOfAcademicYear) &&

                    x.Episodes.Any(episode =>
                        // Include if Started on or before end of activeOnDate year
                        episode.Prices.Any(price => price.StartDate <= endOfAcademicYear) &&

                        // Exclude if withdrawn before start of activeOnDate year
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value < startOfAcademicYear) &&

                        // Exclude if Withdrawn back to start
                        !(episode.WithdrawalDate.HasValue && episode.WithdrawalDate.Value == episode.Prices.Min(p => p.StartDate))));
    }
}
