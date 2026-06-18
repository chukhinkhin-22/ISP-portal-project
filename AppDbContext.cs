using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISP_Portal.API.Data;
using ISP_Portal.API.Models.Dtos;

namespace ISP_Portal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SubscriptionsController(AppDbContext db)
    {
        _db = db;
    }

    // customer's own plan + node info - this is what renders on their dashboard
    [HttpGet("mine")]
    public async Task<ActionResult<List<SubscriptionResponseDto>>> GetMine()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var subs = await _db.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Node)
            .Where(s => s.UserId == userId)
            .ToListAsync();

        return Ok(subs.Select(ToDto));
    }

    // staff need to see a specific customer's subscription when working a ticket
    [HttpGet("{id}")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<SubscriptionResponseDto>> GetById(int id)
    {
        var sub = await _db.Subscriptions
            .Include(s => s.Plan)
            .Include(s => s.Node)
            .FirstOrDefaultAsync(s => s.SubscriptionId == id);

        if (sub is null) return NotFound();

        return Ok(ToDto(sub));
    }

    private static SubscriptionResponseDto ToDto(Models.Subscription s) => new()
    {
        SubscriptionId = s.SubscriptionId,
        PlanName = s.Plan?.PlanName ?? "Unknown",
        SpeedMbps = s.Plan?.SpeedMbps ?? 0,
        MonthlyPrice = s.Plan?.MonthlyPrice ?? 0,
        NodeName = s.Node?.NodeName ?? "Unknown",
        Region = s.Node?.Region ?? "Unknown",
        NodeStatus = s.Node?.Status.ToString() ?? "Unknown",
        StartDate = s.StartDate,
        Status = s.Status.ToString()
    };
}
