using System.ComponentModel.DataAnnotations;
namespace TourismReddit.Api.Models;

public class Category
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<PostCategory> PostCategories { get; set; } = new List<PostCategory>();
}