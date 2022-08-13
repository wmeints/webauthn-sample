using Fido2NetLib;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAuthnSample.Data;
using WebAuthnSample.Models;
using WebAuthnSample.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultDatabase"));
});

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddSingleton<Fido2>(sp =>
{
    var service = new Fido2(new Fido2Configuration()
    {
        Timeout = 30000,
        Origins = new HashSet<string>
        {
            "https://localhost:7140"
        },
        ChallengeSize = 64,
        ServerDomain = "localhost",
        ServerName = "WebAuthnSample",
        TimestampDriftTolerance = 5000,
    });

    return service;
});

builder.Services.AddScoped<IWebAuthnInteractionService, WebAuthnInteractionService>();
builder.Services.AddScoped<PublicKeyCredentialStore>();

var app = builder.Build();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();
app.MapRazorPages();

app.Run();
