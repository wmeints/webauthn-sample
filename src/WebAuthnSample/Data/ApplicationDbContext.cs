using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAuthnSample.Models;

namespace WebAuthnSample.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<PublicKeyCredential> PublicKeyCredentials => Set<PublicKeyCredential>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PublicKeyCredential>().HasKey(x => x.Id);
        builder.Entity<PublicKeyCredential>().Property<string>("ApplicationUserId");
        builder.Entity<PublicKeyCredential>().HasOne(x => x.User)
            .WithMany(x=>x.PublicKeyCredentials)
            .HasForeignKey("ApplicationUserId");
    }
}
