using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace daSSH.Data;

public class CustomBearerTokenHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    DatabaseContext dbContext
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {
    private readonly DatabaseContext _db = dbContext;

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
        var header = Request.Headers["Authorization"].FirstOrDefault()?.Split(' ');
        if (header == null || header.Length != 2 || header[0] != "Bearer") {
            return AuthenticateResult.NoResult();
        }
        var token = header[1];
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.APIToken == token);
        if (user == null) {
            return AuthenticateResult.Fail("Invalid token");
        }

        List<Claim> claims = [new("daSSH-id", user.UserID.ToString())];
        var claimsIdentity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        var ticket = new AuthenticationTicket(claimsPrincipal, "Bearer");
        return AuthenticateResult.Success(ticket);
    }
}
