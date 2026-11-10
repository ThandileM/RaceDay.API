
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required]
        [RegularExpression("^(Participant|Organiser)$",
            ErrorMessage = "Please select Participant or Organiser.")]
        public string Role { get; set; } = "Participant";
    }
}
