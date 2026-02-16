using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Learning.Models
{
    [ExcludeFromCodeCoverage]
    public class Learning
    {
        public Guid Key { get; set; }
        public string Uln { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}