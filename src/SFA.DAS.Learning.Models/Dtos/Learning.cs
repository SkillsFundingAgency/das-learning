using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Models.Dtos;

[ExcludeFromCodeCoverage]
public class Learning
{
    public Guid LearnerKey { get; set; }
    public string Uln { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}