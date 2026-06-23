using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using SFA.DAS.Learning.Command.FreezeLearning;

namespace SFA.DAS.Learning.Functions.Mappers;

public static class FreezeEventMappers
{
    internal static FreezeLearningCommand ToFreezeLearningCommand(ApprenticeshipPausedEvent @event)
    {
        return new FreezeLearningCommand(@event.ApprenticeshipId);
    }

    internal static FreezeLearningCommand ToFreezeLearningCommand(ApprenticeshipStoppedEvent @event)
    {
        return new FreezeLearningCommand(@event.ApprenticeshipId);
    }
}
