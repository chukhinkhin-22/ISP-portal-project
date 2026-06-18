namespace ISP_Portal.API.Models;

public class TicketComment
{
    public int CommentId { get; set; }

    public int TicketId { get; set; }
    public SupportTicket? Ticket { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
