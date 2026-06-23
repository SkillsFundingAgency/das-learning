using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Learning.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Learning.Functions.Mappers;

public static class FreezeEventMappers
{
    internal static ICommand ToFreezeLearningCommand(ApprenticeshipPausedEvent @event)
    {
        throw new NotImplementedException();
    }
}
