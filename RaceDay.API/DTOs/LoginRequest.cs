using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Participant|Organiser)$",
        ErrorMessage = "Role must be Participant or Organiser.")]
    public string Role { get; set; } = string.Empty;
}