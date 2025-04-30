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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostCategory> PostCategories => Set<PostCategory>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentVote> CommentVotes => Set<CommentVote>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<Vote>().HasIndex(v => new { v.UserId, v.PostId }).IsUnique(); // Post Vote unique constraint
        modelBuilder.Entity<Post>().HasOne(p => p.Author).WithMany(u => u.Posts).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasOne(p => p.Country).WithMany().HasForeignKey(p => p.CountryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Vote>().HasOne(v => v.User).WithMany(u => u.Votes).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict); 
        modelBuilder.Entity<Vote>().HasOne(v => v.Post).WithMany(p => p.Votes).HasForeignKey(v => v.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostCategory>().HasKey(pc => new { pc.PostId, pc.CategoryId });
        modelBuilder.Entity<PostCategory>().HasOne(pc => pc.Post).WithMany(p => p.PostCategories).HasForeignKey(pc => pc.PostId);
        modelBuilder.Entity<PostCategory>().HasOne(pc => pc.Category).WithMany(c => c.PostCategories).HasForeignKey(pc => pc.CategoryId);
        modelBuilder.Entity<PostTag>().HasKey(pt => new { pt.PostId, pt.TagId });
        modelBuilder.Entity<PostTag>().HasOne(pt => pt.Post).WithMany(p => p.PostTags).HasForeignKey(pt => pt.PostId);
        modelBuilder.Entity<PostTag>().HasOne(pt => pt.Tag).WithMany(t => t.PostTags).HasForeignKey(pt => pt.TagId);



        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany() 
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany() 
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade); 

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment) 
            .WithMany(p => p.Replies)    
            .HasForeignKey(c => c.ParentCommentId) 
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<CommentVote>()
           .HasIndex(cv => new { cv.UserId, cv.CommentId }) 
           .IsUnique();

        modelBuilder.Entity<CommentVote>()
           .HasOne(cv => cv.User)
           .WithMany() 
           .HasForeignKey(cv => cv.UserId)
           .OnDelete(DeleteBehavior.Restrict); 

        modelBuilder.Entity<CommentVote>()
            .HasOne(cv => cv.Comment)
            .WithMany(c => c.CommentVotes) 
            .HasForeignKey(cv => cv.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

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

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Recommendations" },
            new Category { Id = 2, Name = "Questions" },
            new Category { Id = 3, Name = "Travel Stories" },
            new Category { Id = 4, Name = "Tips & Tricks" },
            new Category { Id = 5, Name = "Food & Drink" }
        );

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
    }
}