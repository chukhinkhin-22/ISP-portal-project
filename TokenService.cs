using ISP_Portal.API.Models;

namespace ISP_Portal.API.Services;

public class RoutingResult
{
    public string DepartmentName { get; set; } = string.Empty;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
}
