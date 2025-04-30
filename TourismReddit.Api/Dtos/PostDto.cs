// Dtos/PostDto.cs
namespace TourismReddit.Api.Dtos
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string AuthorUsername { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string? CountryCode { get; set; }
        // --- Replace string with List<string> ---
        // public string Tags { get; set; } = string.Empty;
        // public string Categories { get; set; } = string.Empty;
        public List<string> CategoryNames { get; set; } = new List<string>();
        public List<string> TagNames { get; set; } = new List<string>();
        // --- End Replace ---
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
        // --- Add New Fields ---
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ImageUrl { get; set; }
        // --- End Add New Fields ---
    }
}