// Filename: Models/Country.cs
using System.ComponentModel.DataAnnotations;

namespace TourismReddit.Api.Models // Ensure namespace matches
{
    public class Country
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(3)]
        public string Code { get; set; } = string.Empty; // e.g., USA, CAN
    }
}