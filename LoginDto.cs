namespace ISP_Portal.API.Models.Dtos;

public class SubscriptionResponseDto
{
    public int SubscriptionId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public int SpeedMbps { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string NodeName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string NodeStatus { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
