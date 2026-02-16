namespace SFA.DAS.Learning.Models;

public class PaymentStatus
{
    public bool IsFrozen { get; set; }
    public string? Reason { get; set; }
    public DateTime? FrozenOn { get; set; }
}
