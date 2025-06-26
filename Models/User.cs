namespace JobPlatform.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public string Address { get; set; } = "";
    public string Role { get; set; } = "Candidate";
    public DateTime CreatedAt { get; set; } = DateTime.Now; // THÊM DÒNG NÀY

    public CandidateProfile? CandidateProfile { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }

    public List<Notification> Notifications { get; set; } = [];
}
