// Models/PostCategory.cs
namespace TourismReddit.Api.Models;
public class PostCategory
{
    public int PostId { get; set; }
    public Post Post { get; set; } = null!; // Navigation properties required
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
}