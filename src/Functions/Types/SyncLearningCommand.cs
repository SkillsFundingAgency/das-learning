using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.Messages.Commands;

/// <summary>
/// SyncLearningCommand is a commitments-originated command that wraps an ApprenticeshipCreatedEvent
/// so that we can unpack in Learning and handle the inner event
/// </summary>
public class SyncLearningCommand
{
    public ApprenticeshipCreatedEvent InnerEvent { get; set; }
}

