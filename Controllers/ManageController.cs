using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController(DatabaseContext db) : Controller {
    private readonly DatabaseContext _db = db;

    public async Task<IActionResult> Instances() {
        var user = await _db.Users
            .Include(u => u.Instances)
            .Include(u => u.SharedInstances)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == long.Parse(User.FindFirstValue("daSSH-id") ?? "0"));
        return View(user);
    }

    public IActionResult NewKeyPair() {
        return View(model: User.FindFirstValue("daSSH-private-key"));
    }
}
