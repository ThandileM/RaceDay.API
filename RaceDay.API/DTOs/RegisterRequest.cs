using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API
{
    public class RegisterRequest
    {
        [Required, MaxLength(50)]
        public string FirstName { get; set; } = "";

        [Required, MaxLength(50)]
        public string LastName { get; set; } = "";

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; } = "";

        [Required, MaxLength(8)]
        public string Password { get; set; } = "";

        [Required]
        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
       public string Role { get; set; } = "Participant";
    }
}
