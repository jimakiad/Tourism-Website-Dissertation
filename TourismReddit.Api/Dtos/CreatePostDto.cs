// Dtos/CreatePostDto.cs
using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Dtos
{
    public class CreatePostDto
    {
        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Body { get; set; } = string.Empty;
        [Required]
        public int CountryId { get; set; }
        // --- Replace string with List<int> ---
        // public string? Tags { get; set; }
        // public string? Categories { get; set; }
        public List<int>? CategoryIds { get; set; } // Optional list of Category IDs
        public List<int>? TagIds { get; set; } // Optional list of Tag IDs
        // --- End Replace ---

        // --- Add New Fields ---
        // Basic validation - could be more specific
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double? Latitude { get; set; }
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double? Longitude { get; set; }
        // Image handled separately via upload endpoint usually
        // --- End Add New Fields ---
    }
}