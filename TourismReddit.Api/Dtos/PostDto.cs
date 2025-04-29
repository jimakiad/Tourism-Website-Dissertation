namespace TourismReddit.Api.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Categories { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
    }
}