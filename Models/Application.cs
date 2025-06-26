using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPlatform.Models;

public class Application
{
    [Key]
    public int Id { get; set; }

    public int JobPostId { get; set; }

    [ForeignKey(nameof(JobPostId))]
    public JobPost JobPost { get; set; } = null!;

    public int CandidateId { get; set; }

    [ForeignKey(nameof(CandidateId))]
    public User Candidate { get; set; } = null!;

    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    public string CoverLetter { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
    public string? InterviewNotes { get; set; } // Dùng để lưu ghi chú, lịch hẹn

}
