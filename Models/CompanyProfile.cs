namespace JobPlatform.Models;

public class CompanyProfile
{
    public int Id { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }

    public string ContactPerson { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;

    public string? Website { get; set; }
    public string? Fanpage { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public List<JobPost> JobPosts { get; set; } = [];

}
