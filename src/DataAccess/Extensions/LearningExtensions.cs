using SFA.DAS.Learning.DataAccess.Entities.Learning;

namespace SFA.DAS.Learning.DataAccess.Extensions;

public static class LearningExtensions
{
    public static Episode GetEpisode(this Entities.Learning.Learning learning)
    {
        var episode = GetLatestActiveEpisode(learning);

        if (episode == null)
        {
            episode = GetLatestEpisode(learning);
        }

        if (episode == null)
            throw new InvalidOperationException("No active episode found");

        return episode;
    }

    public static int GetAgeAtStartOfApprenticeship(this Entities.Learning.Learning learning)
    {
        var startDate = learning.Episodes.SelectMany(e => e.Prices).Min(p => p.StartDate);
        var age = startDate.Year - learning.DateOfBirth.Year;

        if (startDate < learning.DateOfBirth.AddYears(age)) age--;

        return age;
    }

    public static DateTime GetStartDate(this Entities.Learning.Learning learning)
    {
        return learning.Episodes.SelectMany(e => e.Prices).Min(p => p.StartDate);
    }

    public static DateTime GetPlannedEndDate(this Entities.Learning.Learning learning)
    {
        return learning.Episodes.SelectMany(e => e.Prices).Max(p => p.EndDate);
    }

    /// <summary>
    /// NOTE **** This can be replaced in the future with LastDayOfLearning ****
    /// 
    /// This currently returns the Withdraw date which is the last day of learning when the learning was withdrawn
    /// 
    /// This is needed for some of the FM36 block details which are assembled in the earnings outer API
    /// At some point an actual LastDayOfLearning field will be added to the learning entity which will either have 
    /// actual last day of learning on the course or withdrawn date as its value. 
    /// At that time LastDayOfLearning can replace this method
    /// </summary>
    public static DateTime? GetWithdrawnDate(this Entities.Learning.Learning learning)
    {
        if(!learning.WithdrawalRequests.Any())
            return null;

        return learning.WithdrawalRequests.Select(x=>x.LastDayOfLearning)?.Max();
    }

    private static Episode? GetLatestActiveEpisode(Entities.Learning.Learning learning)
    {
        var episode = learning.Episodes
            .MaxBy(x => x.Prices
                .Select(y => (DateTime?)y.StartDate).DefaultIfEmpty(null) //Ensures that we return null if there are no active prices
                .Max());
        return episode;
    }

    private static Episode? GetLatestEpisode(Entities.Learning.Learning learning)
    {
        var episode = learning.Episodes.MaxBy(x => x.Prices.Max(y => y.StartDate));
        return episode;
    }
}
