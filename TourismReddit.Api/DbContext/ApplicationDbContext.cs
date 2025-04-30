// Data/ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using TourismReddit.Api.Models;

namespace TourismReddit.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Vote> Votes => Set<Vote>(); // Post Votes
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    // --- Add Comment DbSets ---
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();
    // --- End Add Comment DbSets ---

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Existing Configurations ---
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Vote>().HasIndex(v => new { v.UserId, v.PostId }).IsUnique(); // Post Vote unique constraint
        modelBuilder.Entity<Post>().HasOne(p => p.Author).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasOne(p => p.Country).WithMany().HasForeignKey(p => p.CountryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vote>().HasOne(v => v.User).WithMany(u => u.Votes).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict); // May need Cascade if User deleted?
        modelBuilder.Entity<Vote>().HasOne(v => v.Post).WithMany(p => p.Votes).HasForeignKey(v => v.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostCategory>().HasKey(pc => new { pc.PostId, pc.CategoryId });
        modelBuilder.Entity<PostCategory>().HasOne(pc => pc.Post).WithMany(p => p.PostCategories).HasForeignKey(pc => pc.PostId);
        modelBuilder.Entity<PostCategory>().HasOne(pc => pc.Category).WithMany(c => c.PostCategories).HasForeignKey(pc => pc.CategoryId);
        modelBuilder.Entity<PostTag>().HasKey(pt => new { pt.PostId, pt.TagId });
        modelBuilder.Entity<PostTag>().HasOne(pt => pt.Post).WithMany(p => p.PostTags).HasForeignKey(pt => pt.PostId);
        modelBuilder.Entity<PostTag>().HasOne(pt => pt.Tag).WithMany(t => t.PostTags).HasForeignKey(pt => pt.TagId);
        // --- End Existing Configurations ---


        // --- Configure Comment ---
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany() // A User can have many comments, but maybe we don't need direct nav from User?
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting user if they have comments? Or Cascade?

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany() // A Post can have many comments, but maybe we don't need direct nav from Post? Define if needed later.
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade); // Delete comments if post is deleted

        // Configure self-referencing relationship for replies
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment) // A comment has one parent (or null)
            .WithMany(p => p.Replies)     // A parent can have many replies
            .HasForeignKey(c => c.ParentCommentId) // Foreign key
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a comment if it has replies? Or Cascade? Careful with cascades here.
                                                // --- End Configure Comment ---


        // --- Configure CommentVote ---
        modelBuilder.Entity<CommentVote>()
           .HasIndex(cv => new { cv.UserId, cv.CommentId }) // User can vote once per comment
           .IsUnique();

        modelBuilder.Entity<CommentVote>()
           .HasOne(cv => cv.User)
           .WithMany() // No direct nav needed from User to CommentVotes usually
           .HasForeignKey(cv => cv.UserId)
           .OnDelete(DeleteBehavior.Restrict); // Or Cascade?

        modelBuilder.Entity<CommentVote>()
            .HasOne(cv => cv.Comment)
            .WithMany(c => c.CommentVotes) // Navigation from Comment to its votes
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

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