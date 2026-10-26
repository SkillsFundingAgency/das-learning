using System.Text.Json.Nodes;
using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.Domain.Apprenticeship;

/// <summary>
/// A change made by an update. <see cref="Old"/> and <see cref="New"/> hold the values before and after the change
/// where they are known: a scalar for a field such as the start date, or an array for a structure such as prices.
/// </summary>
public class LearningChange
{
    public LearningUpdateChanges ChangeType { get; }
    public JsonNode? Old { get; }
    public JsonNode? New { get; }

    /// <summary>
    /// Whether this change must be approved before it takes effect, as opposed to simply being applied.
    /// </summary>
    public bool RequiresApproval { get; }

    public LearningChange(LearningUpdateChanges changeType, JsonNode? old = null, JsonNode? @new = null, bool requiresApproval = false)
    {
        ChangeType = changeType;
        Old = old;
        New = @new;
        RequiresApproval = requiresApproval;
    }

    public static implicit operator LearningChange(LearningUpdateChanges changeType) => new(changeType);
}
