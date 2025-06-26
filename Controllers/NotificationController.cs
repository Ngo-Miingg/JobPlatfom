using JobPlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobPlatform.Controllers;

public class NotificationController(JobDbContext context) : Controller
{
    private readonly JobDbContext _context = context;

    // GET: /Notification
    public IActionResult Index()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null) return RedirectToAction("Login", "Auth");

        var notifications = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToList();

        return View(notifications);
    }

    // POST: /Notification/MarkAsRead/5
    [HttpPost]
    public IActionResult MarkAsRead(int id)
    {
        var notification = _context.Notifications.Find(id);
        if (notification != null)
        {
            notification.IsRead = true;
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    // POST: /Notification/Delete/5
    [HttpPost]
    public IActionResult Delete(int id)
    {
        var notification = _context.Notifications.Find(id);
        if (notification != null)
        {
            _context.Notifications.Remove(notification);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}
