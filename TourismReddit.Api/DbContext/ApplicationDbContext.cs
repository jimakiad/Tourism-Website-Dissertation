using Microsoft.EntityFrameworkCore;
using TourismReddit.Api.Models;

namespace TourismReddit.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Country> Countries => Set<Country>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Unique constraint for votes (one vote per user per post)
        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.PostId })
            .IsUnique();

        // Relationships (EF Core conventions handle most, but explicit is fine)
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade); // Or Restrict depending on rules

        modelBuilder.Entity<Post>()
            .HasOne(p => p.Country)
            .WithMany() // Assuming country doesn't need list of posts
            .HasForeignKey(p => p.CountryId)
            .OnDelete(DeleteBehavior.Restrict); // Don't delete country if posts exist

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict); // User deletion might need custom logic

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Post)
            .WithMany(p => p.Votes)
            .HasForeignKey(v => v.PostId)
            .OnDelete(DeleteBehavior.Cascade); // Delete votes if post is deleted

        // Seed Data (Example)
        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "United States", Code = "US" },
            new Country { Id = 2, Name = "Canada", Code = "CA" },
            new Country { Id = 3, Name = "Mexico", Code = "MX" },
            new Country { Id = 4, Name = "United Kingdom", Code = "GB" },
            new Country { Id = 5, Name = "France", Code = "FR" }
        // Add more as needed
        );
    }
}