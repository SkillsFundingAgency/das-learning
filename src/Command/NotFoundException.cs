using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Command;

[Serializable]
[ExcludeFromCodeCoverage]
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
