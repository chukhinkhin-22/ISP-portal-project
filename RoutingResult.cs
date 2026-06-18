using ISP_Portal.API.Models;

namespace ISP_Portal.API.Services;

// scans a ticket's text and decides which department it should land in,
// plus whether it sounds urgent enough to bump priority. pure logic, no db access -
// that's TicketManager's job, this class just answers "where should this go".
public class TicketRouter : ITicketRouter
{
    private static readonly Dictionary<string, string[]> DepartmentKeywords = new()
    {
        // "disconnecting" dropped - "disconnect" already matches it as a substring,
        // counting both would double-score the same word for no reason
        ["Network_Tier_1"] = new[]
        {
            "slow", "buffering", "ping", "lag", "disconnect",
            "down", "outage", "no internet", "not working", "drops"
        },
        ["Billing_Department"] = new[]
        {
            "bill", "billing", "charge", "charged", "invoice", "payment",
            "refund", "overcharged", "subscription fee", "price"
        }
    };

    // if Network and Billing score equal, Billing wins - a billing word appearing at all
    // is a stronger signal than a network word, since words like "down" can show up
    // in unrelated context ("my account's been down on payments") more easily than
    // something like "invoice" or "refund" showing up by accident
    private static readonly string[] DepartmentPriorityOrder = { "Billing_Department", "Network_Tier_1" };

    // if any of these show up, the ticket jumps to High priority regardless of department
    private static readonly string[] UrgentKeywords =
    {
        "urgent", "completely down", "no internet at all", "asap", "emergency", "still not fixed"
    };

    public RoutingResult Route(string subject, string description)
    {
        var text = $"{subject} {description}".ToLowerInvariant();

        var bestDepartment = "General_Queue";
        var bestScore = 0;

        foreach (var department in DepartmentPriorityOrder)
        {
            var score = DepartmentKeywords[department].Count(keyword => text.Contains(keyword));

            // strictly greater only - so when this department TIES the current best,
            // the one earlier in DepartmentPriorityOrder keeps its spot deliberately
            if (score > bestScore)
            {
                bestScore = score;
                bestDepartment = department;
            }
        }

        var isUrgent = UrgentKeywords.Any(keyword => text.Contains(keyword));

        return new RoutingResult
        {
            DepartmentName = bestDepartment,
            Priority = isUrgent ? TicketPriority.High : TicketPriority.Medium
        };
    }
}
