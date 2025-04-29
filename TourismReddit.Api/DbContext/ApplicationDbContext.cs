// Data/ApplicationDbContext.cs
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
    // --- Add DbSets ---
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    // --- End Add DbSets ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Vote>().HasIndex(v => new { v.UserId, v.PostId }).IsUnique();
        modelBuilder.Entity<Post>().HasOne(p => p.Author).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasOne(p => p.Country).WithMany().HasForeignKey(p => p.CountryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vote>().HasOne(v => v.User).WithMany(u => u.Votes).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vote>().HasOne(v => v.Post).WithMany(p => p.Votes).HasForeignKey(v => v.PostId).OnDelete(DeleteBehavior.Cascade);

        // --- Configure Join Tables ---
        // PostCategory (Many-to-Many)
        modelBuilder.Entity<PostCategory>()
            .HasKey(pc => new { pc.PostId, pc.CategoryId }); // Composite primary key

        modelBuilder.Entity<PostCategory>()
            .HasOne(pc => pc.Post)
            .WithMany(p => p.PostCategories) // Link back to Post's collection
            .HasForeignKey(pc => pc.PostId);

        modelBuilder.Entity<PostCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.PostCategories) // Link back to Category's collection
            .HasForeignKey(pc => pc.CategoryId);

        // PostTag (Many-to-Many)
        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId }); // Composite primary key

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Post)
            .WithMany(p => p.PostTags) // Link back to Post's collection
            .HasForeignKey(pt => pt.PostId);

        modelBuilder.Entity<PostTag>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.PostTags) // Link back to Tag's collection
            .HasForeignKey(pt => pt.TagId);
        // --- End Configure Join Tables ---

        // --- Seed Data ---
        modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "United States", Code = "US" },
            new Country { Id = 2, Name = "Canada", Code = "CA" },
            new Country { Id = 3, Name = "Mexico", Code = "MX" },
            new Country { Id = 4, Name = "United Kingdom", Code = "GB" },
            new Country { Id = 5, Name = "France", Code = "FR" },
            new Country { Id = 6, Name = "Japan", Code = "JP" },
            new Country { Id = 7, Name = "Italy", Code = "IT" },
            new Country { Id = 8, Name = "Greece", Code = "GR" }
        );

        // Add some Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Recommendations" },
            new Category { Id = 2, Name = "Questions" },
            new Category { Id = 3, Name = "Travel Stories" },
            new Category { Id = 4, Name = "Tips & Tricks" },
            new Category { Id = 5, Name = "Food & Drink" }
        );

        // Add some Tags
        modelBuilder.Entity<Tag>().HasData(
           new Tag { Id = 1, Name = "Budget Travel" },
           new Tag { Id = 2, Name = "Luxury Travel" },
           new Tag { Id = 3, Name = "Adventure" },
           new Tag { Id = 4, Name = "Relaxation" },
           new Tag { Id = 5, Name = "Hiking" },
           new Tag { Id = 6, Name = "Beaches" },
           new Tag { Id = 7, Name = "City Break" },
           new Tag { Id = 8, Name = "Culture" },
           new Tag { Id = 9, Name = "Nightlife" }
       );
        // --- End Seed Data ---
    }
}