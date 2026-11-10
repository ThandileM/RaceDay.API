using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class CreateEventViewModel
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Location { get; set; } = "";

        [Range(0.01, 9999.99,
            ErrorMessage = "Distance must be between 0,01 and 9999.99km.")]
        public decimal Distance { get; set; }

        [MaxLength(500)]
        public string? BannerImageUrl { get; set; }
    }
}