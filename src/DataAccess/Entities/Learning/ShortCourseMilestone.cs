using SFA.DAS.Learning.Enums;

namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

[Table("dbo.ShortCourseMilestone")]
[System.ComponentModel.DataAnnotations.Schema.Table("ShortCourseMilestone")]
public class ShortCourseMilestone
{
    [Key]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]
    public Guid Key { get; set; }

    public Guid EpisodeKey { get; set; }
    public Milestone Milestone { get; set; }
}