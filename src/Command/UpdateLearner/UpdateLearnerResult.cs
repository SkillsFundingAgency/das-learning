using System.Text.Json.Nodes;
using SFA.DAS.Learning.Domain.Apprenticeship;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.UpdateLearner
{
    public class UpdateLearnerResult
    {
        /// <summary>
        /// Every change made, with old/new values and whether it requires approval where they apply.
        /// </summary>
        public List<Change> ChangeDetails { get; set; } = [];

        /// <summary>
        /// The change types of <see cref="ChangeDetails"/>, whether or not they require approval.
        /// Retained for consumers that only need to know what changed.
        /// </summary>
        public List<LearningUpdateChanges> Changes => ChangeDetails.Select(x => x.ChangeType).ToList();

        public Guid LearningKey { get; set; }
        public Guid LearningEpisodeKey { get; set; }
        public long ApprovalsApprenticeshipId { get; set; }
        public bool IsApproved { get; set; }

        /// <summary>
        /// The persisted marker on the episode after this update: true while any change is still awaiting approval,
        /// whether it was raised by this update or an earlier one. Distinct from <see cref="Change.RequiresApproval"/>,
        /// which only describes the changes made by this update.
        /// </summary>
        public bool FurtherApprovalNeeded { get; set; }

        public LearningType LearningType { get; set; }
        public int AgeAtStartOfLearning { get; set; }
        public List<EpisodePrice> Prices { get; set; } = [];

        public class Change
        {
            public LearningUpdateChanges ChangeType { get; set; }

            /// <summary>The value before the change: a scalar for a field, an array for a structure such as prices.</summary>
            public JsonNode? Old { get; set; }

            /// <summary>The value after the change, in the same form as <see cref="Old"/>.</summary>
            public JsonNode? New { get; set; }

            public bool RequiresApproval { get; set; }

            public static implicit operator Change(LearningChange x)
            {
                return new Change
                {
                    ChangeType = x.ChangeType,
                    Old = x.Old,
                    New = x.New,
                    RequiresApproval = x.RequiresApproval
                };
            }

            public static implicit operator Change(LearningUpdateChanges changeType)
            {
                return new Change { ChangeType = changeType };
            }
        }

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
