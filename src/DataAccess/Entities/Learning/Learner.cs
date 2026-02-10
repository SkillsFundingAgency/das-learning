namespace SFA.DAS.Learning.DataAccess.Entities.Learning;

#pragma warning disable CS8618

[Table("dbo.Learner")]
[System.ComponentModel.DataAnnotations.Schema.Table("Learner")]
public class Learner
{
    public Learner()
    {
        ApprenticeshipLearnings = new List<ApprenticeshipLearning>();
        ShortCourseLearnings = new List<ShortCourseLearning>();
    }

    public Guid Key { get; set; }
    public string Uln { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public bool HasEHCP { get; set; }
    public bool IsCareLeaver { get; set; }
    public bool CareLeaverEmployerConsentGiven { get; set; }

    public List<ApprenticeshipLearning> ApprenticeshipLearnings { get; set; }
    public List<ShortCourseLearning> ShortCourseLearnings { get; set; }

}

#pragma warning restore CS8618