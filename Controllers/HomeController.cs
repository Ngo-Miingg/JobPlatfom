using JobPlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Controllers;

public class HomeController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // GET: /
    // Trong file Controllers/HomeController.cs
    public IActionResult Index()
    {
        // Lấy các tin tuyển dụng nổi bật (giữ nguyên như cũ)
        var approvedJobs = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .Where(j => j.IsApproved && j.Deadline >= DateTime.Now)
            .OrderByDescending(j => j.PostedDate)
            .Take(9)
            .ToList();

        // --- BẮT ĐẦU THÊM MỚI ---
        // Lấy 8 công ty có nhiều tin tuyển dụng nhất
        var featuredCompanies = _context.CompanyProfiles
            .Where(c => c.JobPosts.Any(j => j.IsApproved)) // Chỉ lấy công ty có tin đã duyệt
            .OrderByDescending(c => c.JobPosts.Count) // Sắp xếp theo số lượng tin đăng
            .Take(8)
            .ToList();
        // Thống kê số lượng việc làm theo ngành nghề
        var industryList = new List<string> { "IT", "Kế toán", "Bán hàng", "Marketing" };
        var industryJobsCount = new Dictionary<string, int>();

        foreach (var industry in industryList)
        {
            var count = _context.JobPosts
                .Count(j => j.IsApproved && j.Deadline >= DateTime.Now && j.InterestedFields.Contains(industry));
            industryJobsCount.Add(industry, count);
        }

        ViewBag.IndustryJobsCount = industryJobsCount;
        // Đưa danh sách công ty vào ViewBag để View có thể dùng
        ViewBag.FeaturedCompanies = featuredCompanies;
        // --- KẾT THÚC THÊM MỚI ---

        return View(approvedJobs);
    }

    // GET: /Home/Details/5
    public IActionResult Details(int id)
    {
        var job = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .FirstOrDefault(j => j.Id == id && j.IsApproved);

        if (job == null) return NotFound();

        return View(job);
    }

    // GET: /Home/Search?keyword=dev&location=Hanoi&industry=IT
    // ===== BẮT ĐẦU CẬP NHẬT =====
    public IActionResult Search(string keyword, string location, string industry)
    {
        var jobsQuery = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .Where(j => j.IsApproved && j.Deadline >= DateTime.Now);

        if (!string.IsNullOrEmpty(keyword))
        {
            jobsQuery = jobsQuery.Where(j => j.Title.Contains(keyword)
                                          || j.Description.Contains(keyword)
                                          || j.CompanyProfile.CompanyName.Contains(keyword));
        }

        if (!string.IsNullOrEmpty(location))
        {
            jobsQuery = jobsQuery.Where(j => j.Location.Contains(location));
        }

        // Thêm điều kiện lọc theo ngành nghề (industry)
        if (!string.IsNullOrEmpty(industry))
        {
            jobsQuery = jobsQuery.Where(j => j.InterestedFields.Contains(industry));
        }

        var result = jobsQuery.OrderByDescending(j => j.PostedDate).ToList();

        // Truyền một tiêu đề riêng cho trang kết quả tìm kiếm
        ViewBag.SearchTitle = $"Tìm thấy {result.Count} việc làm phù hợp";

        return View("Index", result); // Dùng lại view Index để hiển thị kết quả
    }
    // ===== KẾT THÚC CẬP NHẬT =====

    // GET: /Home/About
    public IActionResult About() => View();

    // GET: /Home/Contact
    public IActionResult Contact() => View();
}