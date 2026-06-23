using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipPausedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime PausedOn { get; set; }
    }

    public class ApprenticeshipStoppedEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime AppliedOn { get; set; }
        public bool IsWithDrawnAtStartOfCourse { get; set; }
        public long? LearnerDataId { get; set; }
        public long ProviderId { get; set; }
        public bool FromILR { get; set; }
    }
}
