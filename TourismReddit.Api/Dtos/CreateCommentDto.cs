using System.ComponentModel.DataAnnotations;
namespace TourismReddit.Api.Dtos;

public class CreateCommentDto
{
    [Required]
    public string Body { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; } 
}