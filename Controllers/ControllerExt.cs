using System.Security.Claims;
using daSSH.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace daSSH.Controllers;

public abstract class ControllerExt(DatabaseContext db) : Controller {
    protected DatabaseContext _db = db;

    protected long UserID() {
        return long.Parse(User.FindFirstValue("daSSH-id") ?? "0");
    }

    protected async Task ReissueCookie(long userID, string username, string avatar, params Claim[] additionalClaims) {
        List<Claim> claims = [
            new("daSSH-id", userID.ToString()),
            new("daSSH-username", username),
            new("daSSH-avatar", avatar),
            ..additionalClaims
        ];
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await HttpContext.SignInAsync("Cookies", claimsPrincipal, new AuthenticationProperties {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30),
        });
    }
}
