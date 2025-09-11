using SFA.DAS.Learning.DataTransferObjects;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Command.UpdateLearner
{
    public class UpdateLearnerResult
    {
        public List<LearningUpdateChanges> Changes { get; set; } = [];
        public List<EpisodePrice> Prices { get; set; } = [];
    }
}
