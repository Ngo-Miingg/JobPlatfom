namespace JobPlatform.Models;

public class CandidateProfile
{
    public int Id { get; set; }

    public string ResumeTitle { get; set; } = string.Empty;
    public string Experience { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Skills { get; set; } = string.Empty;

    public string? Certifications { get; set; }
    public string? CvFileUrl { get; set; }
    public string? CoverLetter { get; set; }

    public string DesiredPosition { get; set; } = string.Empty;
    public decimal DesiredSalary { get; set; }
    public string JobType { get; set; } = string.Empty;
    public string PreferredLocation { get; set; } = string.Empty;
    public string InterestedFields { get; set; } = string.Empty;
    public string AvailabilityStatus { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public List<Application> Applications { get; set; } =  [];
}
