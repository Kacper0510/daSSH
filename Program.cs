using Microsoft.EntityFrameworkCore;
using daSSH.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.Discord;

Directory.CreateDirectory("storage");
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DatabaseContextConnection")
    ?? throw new InvalidOperationException("Connection string 'DatabaseContextConnection' not found.");

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite(
    connectionString,
    o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
));

builder.Services
    .AddAuthentication(options => {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = DiscordAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options => {
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.Cookie.Name = "daSSH-login";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.LoginPath = "/SignIn";
    })
    .AddBearerToken(options => options.BearerTokenExpiration = TimeSpan.MaxValue)
    .AddDiscord(options => {
        options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? "none";
        options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_SECRET") ?? "none";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Manage/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

using (var scope = app.Services.CreateScope()) {
    scope.ServiceProvider.GetRequiredService<DatabaseContext>().Database.EnsureCreated();
}

app.MapControllerRoute(
    name: "default",
    pattern: "{action=Index}",
    defaults: new { controller = "Default" }
);
app.MapControllerRoute(
    name: "other",
    pattern: "{controller}/{action}/{id?}"
).WithStaticAssets();

app.Run();
