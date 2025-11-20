using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.UpdateLearner
{
    public class UpdateLearnerResult
    {
        public List<LearningUpdateChanges> Changes { get; set; } = [];
        public Guid LearningEpisodeKey { get; set; }
        public int AgeAtStartOfLearning { get; set; }
        public List<EpisodePrice> Prices { get; set; } = [];

        public class EpisodePrice
        {
            public Guid Key { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal? TrainingPrice { get; set; }
            public decimal? EndPointAssessmentPrice { get; set; }
            public decimal TotalPrice { get; set; }

            public static implicit operator EpisodePrice(EpisodePriceDomainModel x)
            {
                return new EpisodePrice
                {
                    Key = x.Key,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    TrainingPrice = x.TrainingPrice,
                    EndPointAssessmentPrice = x.EndPointAssessmentPrice,
                    TotalPrice = x.TotalPrice
                };
            }
        }
    }
}
