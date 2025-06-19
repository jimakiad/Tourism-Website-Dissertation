using System.ComponentModel.DataAnnotations;
namespace TourismReddit.Api.Models;

public class Tag
{
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}