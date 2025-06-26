using Microsoft.AspNetCore.Mvc;
using JobPlatform.Models;
using JobPlatform.Data;

namespace JobPlatform.Controllers;

public class AuthController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // GET: /Auth/Login
    public IActionResult Login() => View();

    // POST: /Auth/Login
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email && u.PasswordHash == password);
        if (user == null)
        {
            ViewBag.Error = "Sai thông tin đăng nhập";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("UserName", user.FullName);

        return user.Role switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Employer" => RedirectToAction("Dashboard", "Employer"),
            "Candidate" => RedirectToAction("Dashboard", "Candidate"),
            _ => RedirectToAction("Index", "Home")
        };
    }

    // GET: /Auth/Register
    public IActionResult Register() => View();

    // POST: /Auth/Register
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
        {
            ViewBag.Error = "Email đã tồn tại";
            return View();
        }

        user.PasswordHash = user.PasswordHash; // TODO: hash sau
        _context.Users.Add(user);
        _context.SaveChanges();

        if (user.Role == "Candidate")
        {
            _context.CandidateProfiles.Add(new CandidateProfile
            {
                UserId = user.Id,
                ResumeTitle = "Hồ sơ mới",
                Experience = "",
                Education = "",
                Skills = "",
                DesiredPosition = "",
                DesiredSalary = 0,
                JobType = "",
                PreferredLocation = "",
                InterestedFields = "",
                AvailabilityStatus = "Available"
            });
        }
        else if (user.Role == "Employer")
        {
            _context.CompanyProfiles.Add(new CompanyProfile
            {
                UserId = user.Id,
                CompanyName = "Tên công ty",
                ContactPerson = user.FullName,
                ContactEmail = user.Email,
                ContactPhone = user.PhoneNumber
            });
        }

        _context.SaveChanges();
        return RedirectToAction("Login");
    }

    // GET: /Auth/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    // GET: /Auth/Profile
    public IActionResult Profile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        var user = _context.Users.Find(userId);
        return View(user);
    }

    // POST: /Auth/Profile
    [HttpPost]
    public IActionResult Profile(User updatedUser)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        var user = _context.Users.Find(userId);
        if (user == null) return NotFound();

        user.FullName = updatedUser.FullName;
        user.PhoneNumber = updatedUser.PhoneNumber;

        _context.SaveChanges();
        return RedirectToAction("Profile");
    }

    // GET: /Auth/ChangePassword
    public IActionResult ChangePassword() => View();

    // POST: /Auth/ChangePassword
    [HttpPost]
    public IActionResult ChangePassword(string currentPassword, string newPassword)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login");

        var user = _context.Users.Find(userId);
        if (user == null || user.PasswordHash != currentPassword)
        {
            ViewBag.Error = "Mật khẩu hiện tại không đúng";
            return View();
        }

        user.PasswordHash = newPassword; // TODO: hash sau
        _context.SaveChanges();

        ViewBag.Success = "Đổi mật khẩu thành công";
        return View();
    }
}
