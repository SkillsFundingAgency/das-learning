﻿using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.StartDateChange")]
[System.ComponentModel.DataAnnotations.Schema.Table("StartDateChange")]
public class StartDateChange
{
    [Key]
    public Guid Key { get; set; }
    public Guid LearningKey { get; set; }
    public DateTime ActualStartDate { get; set; }
    public DateTime PlannedEndDate { get; set; }
    public string Reason { get; set; } = null!;
    public string? ProviderApprovedBy { get; set; }
    public DateTime? ProviderApprovedDate { get; set; }
    public string? EmployerApprovedBy { get; set; }
    public DateTime? EmployerApprovedDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public ChangeRequestStatus RequestStatus { get; set; }
    public ChangeInitiator? Initiator { get; set; }
    public string RejectReason { get; set; } = null!;
}