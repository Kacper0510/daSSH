using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using daSSH.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;

namespace daSSH.Controllers;

public class DefaultController(DatabaseContext db) : Controller {
    private readonly DatabaseContext _db = db;

    public IActionResult Index() {
        return View();
    }

    public IActionResult SignIn() {
        var redirectUrl = Url.Action("SignInCallback", "Default");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties);
    }

    public async Task<IActionResult> SignInCallback() {
        var discordId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "???";
        var avatar = User.FindFirstValue("urn:discord:avatar:hash");
        var avatarURL = avatar == null
            ? $"https://cdn.discordapp.com/embed/avatars/{(discordId >> 22) % 6}.png"
            : $"https://cdn.discordapp.com/avatars/{discordId}/{avatar}.png";

        string? privKey = null;
        var user = await _db.Users.FirstOrDefaultAsync(u => u.DiscordID == discordId);
        if (user == null) {
            user = new User {
                DiscordID = discordId,
                Username = username,
                Avatar = avatarURL,
                PublicKey = "",
            };
            privKey = await user.GenerateNewKeyPair();
            _db.Users.Add(user);
        } else {
            user.Username = username;
            user.Avatar = avatarURL;
        }
        await _db.SaveChangesAsync();

        var claims = new List<Claim> {
            new("daSSH-id", user.UserID.ToString()),
            new("daSSH-username", username),
            new("daSSH-avatar", avatarURL),
        };
        if (privKey != null) {
            claims.Add(new("daSSH-private-key", privKey));
        }
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await HttpContext.SignInAsync("Cookies", claimsPrincipal, new AuthenticationProperties {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
        });
        return RedirectToAction(privKey == null ? "Instances" : "NewKeyPair", "Manage");
    }

    [ActionName("SignOut")]
    public async Task<IActionResult> SignOutUser() {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Default");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
