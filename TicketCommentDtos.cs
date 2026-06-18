namespace ISP_Portal.API.Models;

public class SupportTicket
{
    public int TicketId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    // not every ticket is tied to a subscription - someone could file a general inquiry
    public int? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }

    // stays null until TicketRouter looks at the description and assigns one
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    public TicketStatus Status { get; set; } = TicketStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
}
