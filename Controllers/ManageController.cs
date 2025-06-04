using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController(DatabaseContext db) : Controller {
    private readonly DatabaseContext _db = db;

    private int UserID() {
        return int.Parse(User.FindFirstValue("daSSH-id") ?? "0");
    }

    public async Task<IActionResult> Instances() {
        var user = await _db.Users
            .Include(u => u.Instances)
            .Include(u => u.SharedInstances)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    public IActionResult NewKeyPair() {
        return View(model: User.FindFirstValue("daSSH-private-key"));
    }

    public async Task<IActionResult> Account() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    public async Task<IActionResult> DeleteAccount() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Default");
    }
}
