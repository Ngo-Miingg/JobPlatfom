using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPlatform.Data;
using JobPlatform.Models;

namespace JobPlatform.Controllers;

public class EmployerController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // Mở file: Controllers/EmployerController.cs

    // File: Controllers/EmployerController.cs

    public IActionResult Dashboard()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Dòng lệnh truy vấn này sẽ tải đầy đủ Company -> JobPosts -> Applications
        var company = _context.CompanyProfiles
            .Include(c => c.JobPosts)
                .ThenInclude(j => j.Applications) // QUAN TRỌNG: Lấy dữ liệu ứng tuyển của mỗi tin đăng
            .FirstOrDefault(c => c.UserId == userId);

        if (company == null)
        {
            // Xử lý trường hợp Nhà tuyển dụng chưa có Profile công ty
            // Có thể chuyển hướng đến trang tạo profile
            return RedirectToAction("CreateProfile", "Employer");
        }

        ViewBag.TotalJobs = company.JobPosts.Count;
        ViewBag.TotalApplications = company.JobPosts.SelectMany(j => j.Applications).Count();

        // Truyền toàn bộ đối tượng company đã có đủ dữ liệu vào View
        return View(company);
    }

    // GET: /Employer/CreateProfile
    public IActionResult CreateProfile() => View();

    // POST: /Employer/CreateProfile
    [HttpPost]
    public IActionResult CreateProfile(CompanyProfile profile)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        profile.UserId = userId.Value;
        _context.CompanyProfiles.Add(profile);
        _context.SaveChanges();

        return RedirectToAction("Dashboard");
    }

    // GET: /Employer/EditProfile
    public IActionResult EditProfile()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var profile = _context.CompanyProfiles.FirstOrDefault(p => p.UserId == userId);
        if (profile == null) return NotFound();

        return View(profile);
    }

    // POST: /Employer/EditProfile
    [HttpPost]
    public IActionResult EditProfile(CompanyProfile updated)
    {
        var existing = _context.CompanyProfiles.Find(updated.Id);
        if (existing == null) return NotFound();

        existing.CompanyName = updated.CompanyName;
        existing.ContactPerson = updated.ContactPerson;
        existing.ContactEmail = updated.ContactEmail;
        existing.ContactPhone = updated.ContactPhone;
        existing.Website = updated.Website;
        existing.Fanpage = updated.Fanpage;

        _context.SaveChanges();
        return RedirectToAction("Dashboard");
    }

    // GET: /Employer/PostJob
    public IActionResult PostJob() => View();

    // POST: /Employer/PostJob
    [HttpPost]
    public IActionResult PostJob(JobPost job)
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        var company = _context.CompanyProfiles.FirstOrDefault(c => c.UserId == userId);
        if (company == null) return NotFound();

        job.CompanyProfileId = company.Id;
        job.PostedDate = DateTime.Now;
        job.IsApproved = false;

        _context.JobPosts.Add(job);
        _context.SaveChanges();

        return RedirectToAction("MyJobs");
    }

    // GET: /Employer/MyJobs
    public IActionResult MyJobs()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Action này đã có sẵn .ThenInclude() từ trước, ta chỉ đảm bảo nó đúng
        var company = _context.CompanyProfiles
            .Include(c => c.JobPosts)
            .ThenInclude(j => j.Applications) // Đảm bảo dòng này tồn tại
            .FirstOrDefault(c => c.UserId == userId);

        if (company == null) return NotFound();

        return View(company.JobPosts.OrderByDescending(j => j.PostedDate).ToList());
    }

    // GET: /Employer/EditJob/5
    public IActionResult EditJob(int id)
    {
        var job = _context.JobPosts.Find(id);
        if (job == null) return NotFound();
        return View(job);
    }

    // POST: /Employer/EditJob
    [HttpPost]
    public IActionResult EditJob(JobPost updated)
    {
        var job = _context.JobPosts.Find(updated.Id);
        if (job == null) return NotFound();

        job.Title = updated.Title;
        job.Description = updated.Description;
        job.Salary = updated.Salary;
        job.Deadline = updated.Deadline;

        _context.SaveChanges();
        return RedirectToAction("MyJobs");
    }

    // GET: /Employer/DeleteJob/5
    public IActionResult DeleteJob(int id)
    {
        var job = _context.JobPosts.Find(id);
        if (job == null) return NotFound();

        _context.JobPosts.Remove(job);
        _context.SaveChanges();
        return RedirectToAction("MyJobs");
    }

    // GET: /Employer/Applications/5
    public IActionResult Applications(int id)
    {
        var job = _context.JobPosts
            .Include(j => j.Applications)
            .ThenInclude(a => a.Candidate)
            .FirstOrDefault(j => j.Id == id);

        if (job == null) return NotFound();

        return View(job.Applications);
    }

    // POST: /Employer/UpdateApplicationStatus
    [HttpPost]
    public IActionResult UpdateApplicationStatus(int applicationId, string status, string interviewNotes)
    {
        var app = _context.Applications
                        .Include(a => a.JobPost)
                            .ThenInclude(j => j.CompanyProfile)
                        .Include(a => a.Candidate)
                        .FirstOrDefault(a => a.Id == applicationId);

        if (app == null) return NotFound();

        app.Status = status;
        app.InterviewNotes = interviewNotes; // <-- LƯU LẠI LỜI NHẮN

        string notificationMessage = string.Empty;
        switch (status.ToLower())
        {
            case "viewed":
                notificationMessage = $"Nhà tuyển dụng {app.JobPost.CompanyProfile.CompanyName} đã xem hồ sơ của bạn cho vị trí '{app.JobPost.Title}'.";
                break;
            case "approved": // Đổi thành "Mời phỏng vấn"
                notificationMessage = $"Chúc mừng! Bạn được mời phỏng vấn cho vị trí '{app.JobPost.Title}'. Lời nhắn từ nhà tuyển dụng: '{interviewNotes}'";
                break;
            case "rejected":
                notificationMessage = $"Cảm ơn bạn đã ứng tuyển vị trí '{app.JobPost.Title}'. Rất tiếc hồ sơ của bạn chưa phù hợp.";
                break;
        }

        if (!string.IsNullOrEmpty(notificationMessage))
        {
            var notification = new Notification
            {
                UserId = app.CandidateId,
                Message = notificationMessage,
                IsRead = false,
                CreatedAt = DateTime.Now,
                User = app.Candidate
            };
            _context.Notifications.Add(notification);
        }

        _context.SaveChanges();

        return RedirectToAction("Applications", new { id = app.JobPostId });
    }
    // GET: /Employer/ApplicationDetails/5
    // (id ở đây là Application.Id)
    public IActionResult ApplicationDetails(int id)
    {
        // Tải đơn ứng tuyển, bao gồm:
        // 1. Thông tin người ứng tuyển (Candidate)
        // 2. Kèm theo hồ sơ chi tiết của người đó (CandidateProfile)
        // 3. Thông tin công việc mà họ đã ứng tuyển (JobPost)
        var application = _context.Applications
            .Include(a => a.Candidate)
                .ThenInclude(c => c.CandidateProfile)
            .Include(a => a.JobPost)
            .FirstOrDefault(a => a.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        // Gửi toàn bộ đối tượng application đến View
        return View(application);
    }
}
