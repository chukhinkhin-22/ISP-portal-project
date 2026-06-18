namespace ISP_Portal.API.Models;

public class Subscription
{
    public int SubscriptionId { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int PlanId { get; set; }
    public Plan? Plan { get; set; }

    public int NodeId { get; set; }
    public NetworkNode? Node { get; set; }

    public DateTime StartDate { get; set; }

    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public ICollection<SupportTicket> Tickets { get; set; } = new List<SupportTicket>();
}
