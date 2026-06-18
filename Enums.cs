using System.ComponentModel.DataAnnotations;

namespace ISP_Portal.API.Models.Dtos;

public class CreateTicketDto
{
    [Required, MaxLength(150)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    // optional - some tickets are general questions, not tied to a subscription
    public int? SubscriptionId { get; set; }
}
