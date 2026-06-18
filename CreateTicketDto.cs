using System.ComponentModel.DataAnnotations;

namespace ISP_Portal.API.Models.Dtos;

public class RegisterDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Address { get; set; }
}
