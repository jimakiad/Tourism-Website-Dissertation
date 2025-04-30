// Dtos/CreateCommentDto.cs
using System.ComponentModel.DataAnnotations;
namespace TourismReddit.Api.Dtos;

public class CreateCommentDto
{
    [Required]
    public string Body { get; set; } = string.Empty;
    // PostId will come from the route
    public int? ParentCommentId { get; set; } // Optional parent for replies
}