using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAuthnSample.Data;
using WebAuthnSample.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultDatabase"));
});

builder.Services.AddSession();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllers();
app.MapRazorPages();

app.UseWelcomePage();

app.Run();
