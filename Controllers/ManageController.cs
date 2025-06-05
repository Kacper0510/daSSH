using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace daSSH.Controllers;

[Authorize(AuthenticationSchemes = "Cookies")]
public class ManageController(DatabaseContext db) : ControllerExt(db) {
    public async Task<IActionResult> Instances() {
        var user = await _db.Users
            .Include(u => u.Instances)
            .Include(u => u.SharedInstances)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    public IActionResult NewKeyPair() {
        foreach (var i in User.Identities) {
            Console.WriteLine($"{i.AuthenticationType} - {string.Join(',', i.Claims.Select(c => $"{c.Type}={c.Value}"))}");
        }
        return View(model: User.FindFirstValue("daSSH-private-key"));
    }

    public IActionResult NewToken() {
        return View(model: User.FindFirstValue("daSSH-token"));
    }

    public async Task<IActionResult> Account() {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        return View(user);
    }

    [HttpPost]
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

    public async Task<IActionResult> RegenerateKeyPair() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        var privKey = await user.GenerateNewKeyPair();
        await _db.SaveChangesAsync();

        await ReissueCookie(
            user.UserID,
            user.Username,
            user.Avatar,
            new Claim("daSSH-private-key", privKey)
        );

        return RedirectToAction("NewKeyPair");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteToken() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        user.APIToken = null;
        await _db.SaveChangesAsync();

        return RedirectToAction("Account");
    }

    public async Task<IActionResult> GenerateToken() {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }
        user.GenerateNewToken();
        await _db.SaveChangesAsync();

        await ReissueCookie(
            user.UserID,
            user.Username,
            user.Avatar,
            new Claim("daSSH-token", user.APIToken!)
        );

        return RedirectToAction("NewToken");
    }
}
