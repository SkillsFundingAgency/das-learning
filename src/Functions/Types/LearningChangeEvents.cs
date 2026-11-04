using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Messages.Events;

// Local copy of Approvals' events (das-commitments, SFA.DAS.CommitmentsV2.Messages.Events).

public class LearningChangeApprovedEvent : LearningChangeEvent { }

public class LearningChangeRejectedEvent : LearningChangeEvent { }

public class LearningChangeEvent
{
    public Guid LearningKey { get; set; }
    public long ApprenticeshipId { get; set; }
    public List<Change> Changes { get; set; } = new(); //List as per spec, not a dictionary as per current Approvals implementation.

    public class Change
    {
        public string ChangeType { get; set; }
        public ChangeData Data { get; set; }
    }

    public class ChangeData
    {
        public string Old { get; set; }
        public string New { get; set; }
    }
}
