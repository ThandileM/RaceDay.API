using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record UserResponseDto(int UserId, string FirstName, string LastName, string Email, string? PhoneNumber, string Role, string? ProfilePictureUrl, DateTime CreatedAt);
public class CreateUserDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(150)] public string Email { get; set; } = "";
    [Required, MinLength(8)] public string Password { get; set; } = "";
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [Required, RegularExpression("^(Organiser|Participant)$")] public string Role { get; set; } = "Participant";
    [MaxLength(500)] public string? ProfilePictureUrl { get; set; }
}
public class UpdateUserDto
{
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress, MaxLength(150)] public string Email { get; set; } = "";
    [MaxLength(20)] public string? PhoneNumber { get; set; }
    [Required, RegularExpression("^(Organiser|Participant)$")] public string Role { get; set; } = "Participant";
    [MaxLength(500)] public string? ProfilePictureUrl { get; set; }
}
public class CreateEventDto
{
    [Range(1, int.MaxValue)] public int OrganiserId { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Required] public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";

    [Range(typeof(decimal), "0.01", "9999.99",
    ParseLimitsInInvariantCulture = true)]
    public decimal Distance { get; set; }
    [MaxLength(500)] public string? BannerImageUrl { get; set; }
}
public class UpdateEventDto : CreateEventDto { }
public record EventResponseDto(int EventId, int OrganiserId, string Name, string? Description, DateTime EventDate, string Location, decimal Distance, string? BannerImageUrl, DateTime CreatedAt);
public class CreateCategoryDto
{
    [Range(1, int.MaxValue)] public int EventId { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = "";
    [MaxLength(255)] public string? Description { get; set; }
}
public class CreateEnrolmentDto
{
    [Range(1, int.MaxValue)] public int ParticipantId { get; set; }
    [Range(1, int.MaxValue)] public int EventId { get; set; }
    [Range(1, int.MaxValue)] public int CategoryId { get; set; }
    [Range(1, int.MaxValue)] public int StatusId { get; set; } = 1;
}
public class UpdateEnrolmentStatusDto { [Range(1, int.MaxValue)] public int StatusId { get; set; } }
public class CreateResultDto
{
    [Range(1, int.MaxValue)] public int EnrolmentId { get; set; }
    [Range(1, int.MaxValue)] public int RecordedById { get; set; }
    [Required] public TimeSpan FinishTime { get; set; }
    [Range(1, int.MaxValue)] public int FinishingPosition { get; set; }
    [Range(1, int.MaxValue)] public int TotalFinishers { get; set; }
}
