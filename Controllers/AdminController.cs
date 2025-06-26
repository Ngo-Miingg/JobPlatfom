using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPlatform.Data;
using JobPlatform.Models;

namespace JobPlatform.Controllers;

public class AdminController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // GET: /Admin/Dashboard
    public IActionResult Dashboard()
    {
        var totalUsers = _context.Users.Count();
        var totalJobs = _context.JobPosts.Count();
        var totalApplications = _context.Applications.Count();
        var pendingJobs = _context.JobPosts.Count(j => !j.IsApproved);

        ViewBag.TotalUsers = totalUsers;
        ViewBag.TotalJobs = totalJobs;
        ViewBag.TotalApplications = totalApplications;
        ViewBag.PendingJobs = pendingJobs;

        return View();
    }

    // GET: /Admin/Users
    public IActionResult Users()
    {
        var users = _context.Users.ToList();
        return View(users);
    }

    // GET: /Admin/DeleteUser/5
    public IActionResult DeleteUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction("Users");
    }

    public IActionResult Applications()
    {
        var applications = _context.Applications
            .Include(a => a.JobPost) // Lấy thông tin tin tuyển dụng
            .ThenInclude(jp => jp.CompanyProfile) // Lấy thông tin công ty từ tin tuyển dụng
            .Include(a => a.Candidate) // Lấy thông tin người ứng tuyển
            .OrderByDescending(a => a.ApplicationDate)
            .ToList();

        return View(applications);
    }
    public IActionResult JobPosts()
    {
        // QUAN TRỌNG: Phải có .Include(j => j.Applications)
        var jobs = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .Include(j => j.Applications) // Tải kèm danh sách ứng tuyển
            .OrderByDescending(j => j.PostedDate)
            .ToList();

        return View(jobs);
    }

    // GET: /Admin/Approve/5
    public IActionResult Approve(int id)
    {
        var job = _context.JobPosts.Find(id);
        if (job == null) return NotFound();

        job.IsApproved = true;
        _context.SaveChanges();

        return RedirectToAction("JobPosts");
    }

    // GET: /Admin/DeleteJob/5
    public IActionResult DeleteJob(int id)
    {
        var job = _context.JobPosts.Find(id);
        if (job == null) return NotFound();

        _context.JobPosts.Remove(job);
        _context.SaveChanges();
        return RedirectToAction("JobPosts");
    }

    // GET: /Admin/Candidates
    public IActionResult Candidates()
    {
        var candidates = _context.Users
            .Include(u => u.CandidateProfile)
            .Where(u => u.Role == "Candidate")
            .ToList();

        return View(candidates);
    }

    // GET: /Admin/Employers
    public IActionResult Employers()
    {
        var employers = _context.Users
            .Include(u => u.CompanyProfile)
            .Where(u => u.Role == "Employer")
            .ToList();

        return View(employers);
    }
    // GET: /Admin/JobDetails/5
    public IActionResult JobDetails(int id)
    {
        // Tải tin tuyển dụng theo ID, bao gồm:
        // 1. Thông tin công ty đăng tin (CompanyProfile)
        // 2. Danh sách các đơn ứng tuyển (Applications)
        // 3. Với mỗi đơn ứng tuyển, tải kèm thông tin của ứng viên (Candidate)
        var jobPost = _context.JobPosts
            .Include(j => j.CompanyProfile)
            .Include(j => j.Applications)
                .ThenInclude(a => a.Candidate)
            .FirstOrDefault(j => j.Id == id);

        if (jobPost == null)
        {
            return NotFound();
        }

        return View(jobPost);
    }
}
