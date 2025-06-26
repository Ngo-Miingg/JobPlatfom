using JobPlatform.Models;
namespace JobPlatform.Models;

public class JobPost
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Salary { get; set; }
    public DateTime Deadline { get; set; }
    public DateTime PostedDate { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } = false;
    public string Requirements { get; set; }
    public string Benefits { get; set; }
    public string Location { get; set; } = ""; // ➕ thêm dòng này
    // Thêm các thuộc tính dưới đây nếu chưa có!
    public string JobType { get; set; } = "";           // VD: Toàn thời gian, Bán thời gian
    public string InterestedFields { get; set; } = "";  // VD: "IT;Kế toán;Marketing"
    public int CompanyProfileId { get; set; }
    public CompanyProfile CompanyProfile { get; set; } = null!;
    public List<Application> Applications { get; set; } = [];
}
