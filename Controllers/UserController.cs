using JobPlatform.Data;
using JobPlatform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Controllers;

public class UserController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

    // GET: /User/Profile
    public IActionResult Profile()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var user = _context.Users
            .Include(u => u.CandidateProfile)
            .Include(u => u.CompanyProfile)
            .FirstOrDefault(u => u.Id == userId);

        if (user == null) return NotFound();

        return View(user);
    }

    // GET: /User/Edit
    public IActionResult Edit()
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var user = _context.Users.Find(userId);
        if (user == null) return NotFound();

        // Action này trả về View với model là User, hoàn toàn chính xác.
        return View(user);
    }

    // POST: /User/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(User updatedUser)
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Chỉ cập nhật những trường cho phép
        var userToUpdate = _context.Users.Find(userId.Value);
        if (userToUpdate == null) return NotFound();

        userToUpdate.FullName = updatedUser.FullName;
        userToUpdate.PhoneNumber = updatedUser.PhoneNumber;
        userToUpdate.Address = updatedUser.Address;
        userToUpdate.AvatarUrl = updatedUser.AvatarUrl; // Cân nhắc thêm trường upload ảnh

        _context.SaveChanges();
        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
        return RedirectToAction("Profile");
    }

    // GET: /User/ChangePassword
    public IActionResult ChangePassword() => View();

    // POST: /User/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var userId = GetUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var user = _context.Users.FirstOrDefault(u => u.Id == userId);

        // TODO: Sẽ thay thế bằng logic hash password
        if (user == null || user.PasswordHash != currentPassword)
        {
            ViewBag.Error = "Mật khẩu hiện tại không đúng.";
            return View();
        }

        if (newPassword != confirmPassword)
        {
            ViewBag.Error = "Mật khẩu mới không khớp.";
            return View();
        }

        user.PasswordHash = newPassword;
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Profile");
    }
}