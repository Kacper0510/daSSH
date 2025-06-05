using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using daSSH.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using daSSH.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace daSSH.Controllers;

public class DefaultController(DatabaseContext db) : ControllerExt(db) {
    public IActionResult Index() {
        if (User.FindFirst("daSSH-id") != null) {
            return RedirectToAction("Instances", "Manage");
        }
        return View();
    }

    public IActionResult SignIn() {
        var redirectUrl = Url.Action("SignInCallback", "Default");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties);
    }

    public IActionResult Manage() {
        return RedirectToActionPermanent("Instances", "Manage");
    }

    [Authorize(AuthenticationSchemes = "Discord")]
    public async Task<IActionResult> SignInCallback() {
        var discordId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var username = User.FindFirstValue(ClaimTypes.Name) ?? "???";
        var avatar = User.FindFirstValue("urn:discord:avatar:hash");
        var avatarURL = avatar == null
            ? $"https://cdn.discordapp.com/embed/avatars/{(discordId >> 22) % 6}.png"
            : $"https://cdn.discordapp.com/avatars/{discordId}/{avatar}.png";

        var user = await _db.Users.FirstOrDefaultAsync(u => u.DiscordID == discordId);
        var newAccount = user == null;
        if (newAccount) {
            user = new User {
                DiscordID = discordId,
                Username = username,
                Avatar = avatarURL,
                PublicKey = "",
            };
            _db.Users.Add(user);
        } else {
            user!.Username = username;
            user!.Avatar = avatarURL;
        }
        await _db.SaveChangesAsync();

        await ReissueCookie(user.UserID, user.Username, user.Avatar);

        return RedirectToAction(newAccount ? "RegenerateKeyPair" : "Instances", "Manage");
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
