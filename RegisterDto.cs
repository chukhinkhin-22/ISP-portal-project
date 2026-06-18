using System.ComponentModel.DataAnnotations;

namespace ISP_Portal.API.Models;

public class User
{
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Customer;

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // one user can have many subscriptions, tickets, comments
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
}
