using Microsoft.AspNetCore.Mvc;
using JobPlatform.Data;
using JobPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Controllers;

public class CandidateController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // GET: /Candidate/Dashboard
    public IActionResult Dashboard()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var profile = _context.CandidateProfiles
            .Include(p => p.User) // Include thông tin User để hiển thị tên
            .FirstOrDefault(p => p.UserId == userId);

        // KIỂM TRA NẾU PROFILE KHÔNG TỒN TẠI
        if (profile == null)
        {
            // TỰ ĐỘNG TẠO PROFILE MỚI, MẶC ĐỊNH CHO USER
            var newProfile = new CandidateProfile
            {
                UserId = userId.Value,
                ResumeTitle = "Hồ sơ của tôi",
                // Các trường khác có thể để mặc định hoặc rỗng
            };
            _context.CandidateProfiles.Add(newProfile);
            _context.SaveChanges();

            // Tải lại profile vừa tạo để có đủ thông tin (bao gồm cả User)
            profile = _context.CandidateProfiles
                .Include(p => p.User)
                .FirstOrDefault(p => p.UserId == userId);
        }

        // Tại thời điểm này, 'profile' chắc chắn không null
        return View(profile);
    }

    // GET: /Candidate/EditProfile
    public IActionResult EditProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var profile = _context.CandidateProfiles
            .FirstOrDefault(p => p.UserId == userId);
        return View(profile);
    }

    // POST: /Candidate/EditProfile
    [HttpPost]
    public IActionResult EditProfile(CandidateProfile updatedProfile)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var profile = _context.CandidateProfiles.FirstOrDefault(p => p.UserId == userId);
        if (profile == null) return NotFound();

        // ===== BẮT ĐẦU CẬP NHẬT LOGIC =====
        // Sử dụng toán tử ?? để đảm bảo không gán giá trị null cho các trường NOT NULL
        profile.ResumeTitle = updatedProfile.ResumeTitle ?? string.Empty;
        profile.Experience = updatedProfile.Experience ?? string.Empty;
        profile.Education = updatedProfile.Education ?? string.Empty;
        profile.Skills = updatedProfile.Skills ?? string.Empty;
        profile.DesiredPosition = updatedProfile.DesiredPosition ?? string.Empty;
        profile.JobType = updatedProfile.JobType ?? string.Empty;
        profile.PreferredLocation = updatedProfile.PreferredLocation ?? string.Empty;
        profile.InterestedFields = updatedProfile.InterestedFields ?? string.Empty; // Sửa trực tiếp lỗi
        profile.AvailabilityStatus = updatedProfile.AvailabilityStatus ?? string.Empty;

        // Các trường có thể null thì giữ nguyên
        profile.Certifications = updatedProfile.Certifications;
        profile.CvFileUrl = updatedProfile.CvFileUrl;
        profile.CoverLetter = updatedProfile.CoverLetter;

        // Trường số
        profile.DesiredSalary = updatedProfile.DesiredSalary;

        _context.SaveChanges(); // Lệnh này sẽ không còn gây lỗi
                                // ===== KẾT THÚC CẬP NHẬT LOGIC =====

        TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
        return RedirectToAction("Dashboard");
    }
    // GET: /Candidate/Jobs
    public IActionResult Jobs()
    {
        var jobs = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .Where(j => j.IsApproved && j.Deadline >= DateTime.Now)
            .OrderByDescending(j => j.PostedDate)
            .ToList();
        return View(jobs);
    }

    // GET: /Candidate/Apply/5
    public IActionResult Apply(int id)
    {
        var job = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .FirstOrDefault(j => j.Id == id);
        if (job == null) return NotFound();
        return View(job);
    }

    // POST: /Candidate/Apply/5
    [HttpPost]
    public IActionResult Apply(int id, string coverLetter)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        if (_context.Applications.Any(a => a.JobPostId == id && a.CandidateId == userId))
        {
            ViewBag.Error = "Bạn đã ứng tuyển công việc này rồi.";
            return RedirectToAction("Jobs");
        }

        var application = new Application
        {
            JobPostId = id,
            CandidateId = userId.Value,
            ApplicationDate = DateTime.Now,
            CoverLetter = coverLetter,
            Status = "Pending"
        };

        _context.Applications.Add(application);
        _context.SaveChanges();

        return RedirectToAction("Dashboard");
    }

    // GET: /Candidate/Applications
    public IActionResult Applications()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var apps = _context.Applications
            .Include(a => a.JobPost)
                .ThenInclude(j => j.CompanyProfile)
            .Include(a => a.Candidate) // <-- THÊM DÒNG NÀY ĐỂ TẢI THÔNG TIN ỨNG VIÊN
            .Where(a => a.CandidateId == userId)
            .OrderByDescending(a => a.ApplicationDate)
            .ToList();

        return View(apps);
    }
}
