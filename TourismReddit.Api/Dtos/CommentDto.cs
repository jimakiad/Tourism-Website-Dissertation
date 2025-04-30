namespace TourismReddit.Api.Dtos;

public class CommentDto
{
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int PostId { get; set; }
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Score { get; set; }
    public List<CommentDto> Replies { get; set; } = new List<CommentDto>();
}