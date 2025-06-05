using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using daSSH.Models;

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

    [HttpPost]
    public async Task<IActionResult> CreateInstance(string name, bool forward, ushort port, bool publicPort) {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserID == UserID());
        if (user == null) {
            return NotFound();
        }

        var instance = new Instance {
            Name = name,
            Owner = user,
        };
        _db.Instances.Add(instance);
        if (forward) {
            var portForward = new PortForward {
                Port = port,
                Public = publicPort,
                Instance = instance
            };
            _db.Forwards.Add(portForward);
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Instances");
    }
}
