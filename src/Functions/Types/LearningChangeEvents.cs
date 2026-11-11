using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Events;

// Local copy of Approvals' events (das-commitments, SFA.DAS.CommitmentsV2.Messages.Events).

public class LearningChangeApprovedEvent : LearningChangeEvent { }

public class LearningChangeRejectedEvent : LearningChangeEvent
{
    public string Reason { get; set; } // Unused.
}

public class LearningChangeEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public List<Change> Changes { get; set; } = new(); //List as per spec, not a dictionary as per current Approvals implementation.
    public List<Price> Prices { get; set; }

    public class Change
    {
        public string ChangeType { get; set; }
        public string ApprovalStatus { get; set; } // Unused: the event type is the decision, applied to all changes.
        public ChangeData Data { get; set; }
    }

    public class ChangeData
    {
        public string Old { get; set; }
        public string New { get; set; }
    }

    public class Price
    {
        public decimal? TrainingPrice { get; set; }
        public decimal? AssessmentPrice { get; set; }
        public string EffectiveFrom { get; set; }
    }
}
