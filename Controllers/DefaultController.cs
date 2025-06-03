using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using daSSH.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace daSSH.Controllers;

public class DefaultController : Controller {
    public IActionResult Index() {
        return View();
    }

    public IActionResult SignIn() {
        var redirectUrl = Url.Action("SignInCallback", "Default");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties);
    }

    public async Task<IActionResult> SignInCallback() {
        var result = await HttpContext.AuthenticateAsync("Cookies");
        if (!result.Succeeded) {
            return RedirectToAction("Error", "Default");
        }

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var discordId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        var username = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var avatar = claims?.FirstOrDefault(c => c.Type == "urn:discord:avatar:hash")?.Value;

        var userInfo = new {
            DiscordId = discordId,
            Username = username,
            AvatarUrl = $"https://cdn.discordapp.com/avatars/{discordId}/{avatar}.png"
        };

        return Json(userInfo);
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
