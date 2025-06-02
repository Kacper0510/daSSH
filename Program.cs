using Microsoft.EntityFrameworkCore;
using daSSH.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DatabaseContextConnection")
    ?? throw new InvalidOperationException("Connection string 'DatabaseContextConnection' not found.");

builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlite(connectionString));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.Cookie.Expiration = TimeSpan.FromHours(1);
        options.Cookie.Name = "dassh_login";
    })
    .AddBearerToken(options => options.BearerTokenExpiration = TimeSpan.MaxValue)
    .AddDiscord(options => {
        options.ClientId = Environment.GetEnvironmentVariable("DISCORD_CLIENT_ID") ?? "";
        options.ClientSecret = Environment.GetEnvironmentVariable("DISCORD_SECRET") ?? "";
    });

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
