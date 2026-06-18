using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ISP_Portal.API.Data;
using ISP_Portal.API.Models;
using ISP_Portal.API.Models.Dtos;
using ISP_Portal.API.Services;

namespace ISP_Portal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITicketManager _ticketManager;

    public TicketsController(AppDbContext db, ITicketManager ticketManager)
    {
        _db = db;
        _ticketManager = ticketManager;
    }

    // customer submits a ticket - routing happens automatically inside TicketManager
    [HttpPost]
    public async Task<ActionResult<TicketResponseDto>> Submit(CreateTicketDto dto)
    {
        var userId = GetCurrentUserId();
        var ticket = await _ticketManager.SubmitTicketAsync(userId, dto);

        var withDepartment = await _db.SupportTickets
            .Include(t => t.Department)
            .FirstAsync(t => t.TicketId == ticket.TicketId);

        return Ok(ToDto(withDepartment));
    }

    // customer sees their own ticket history
    [HttpGet]
    public async Task<ActionResult<List<TicketResponseDto>>> GetMyTickets()
    {
        var userId = GetCurrentUserId();

        var tickets = await _db.SupportTickets
            .Include(t => t.Department)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tickets.Select(ToDto));
    }

    // staff queue - this is where the routing engine actually pays off.
    // tickets already arrive sorted into departments and by priority instead
    // of staff having to read every single one to figure out where it belongs.
    [HttpGet("queue")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<List<TicketResponseDto>>> GetQueue(
        [FromQuery] string? department, [FromQuery] string? status)
    {
        var query = _db.SupportTickets.Include(t => t.Department).AsQueryable();

        if (!string.IsNullOrEmpty(department))
            query = query.Where(t => t.Department != null && t.Department.DepartmentName == department);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TicketStatus>(status, true, out var statusEnum))
            query = query.Where(t => t.Status == statusEnum);

        var tickets = await query
            .OrderByDescending(t => t.Priority) // High sorts first since enum order is Low, Medium, High
            .ThenBy(t => t.CreatedAt)
            .ToListAsync();

        return Ok(tickets.Select(ToDto));
    }

    // anyone involved with the ticket can read the thread - customer who filed it, or staff/admin
    [HttpGet("{id}/comments")]
    public async Task<ActionResult<List<CommentResponseDto>>> GetComments(int id)
    {
        var ticket = await _db.SupportTickets.FindAsync(id);
        if (ticket is null) return NotFound();

        if (!CanAccessTicket(ticket)) return Forbid();

        var comments = await _db.TicketComments
            .Include(c => c.User)
            .Where(c => c.TicketId == id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(comments.Select(ToCommentDto));
    }

    // adds a reply to the thread - either the customer following up, or staff responding
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentResponseDto>> AddComment(int id, CreateCommentDto dto)
    {
        var ticket = await _db.SupportTickets.FindAsync(id);
        if (ticket is null) return NotFound();

        if (!CanAccessTicket(ticket)) return Forbid();

        var comment = new TicketComment
        {
            TicketId = id,
            UserId = GetCurrentUserId(),
            Message = dto.Message
        };

        _db.TicketComments.Add(comment);
        await _db.SaveChangesAsync();

        var saved = await _db.TicketComments.Include(c => c.User).FirstAsync(c => c.CommentId == comment.CommentId);
        return Ok(ToCommentDto(saved));
    }

    // staff move a ticket through its lifecycle - Open -> In_Progress -> Resolved -> Closed
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Staff,Admin")]
    public async Task<ActionResult<TicketResponseDto>> UpdateStatus(int id, UpdateTicketStatusDto dto)
    {
        var ticket = await _db.SupportTickets.Include(t => t.Department).FirstOrDefaultAsync(t => t.TicketId == id);
        if (ticket is null) return NotFound();

        if (!Enum.TryParse<TicketStatus>(dto.Status, true, out var newStatus))
            return BadRequest("invalid status value");

        ticket.Status = newStatus;

        // stamp resolution time once, but don't keep overwriting it if it bounces back to resolved again
        if (newStatus == TicketStatus.Resolved && ticket.ResolvedAt is null)
            ticket.ResolvedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(ticket));
    }

    // customer can only touch their own ticket, staff/admin can touch anything
    private bool CanAccessTicket(SupportTicket ticket)
    {
        if (User.IsInRole("Staff") || User.IsInRole("Admin")) return true;
        return ticket.UserId == GetCurrentUserId();
    }

    private static CommentResponseDto ToCommentDto(TicketComment c) => new()
    {
        CommentId = c.CommentId,
        UserId = c.UserId,
        AuthorName = c.User?.FullName ?? "Unknown",
        AuthorRole = c.User?.Role.ToString() ?? "Unknown",
        Message = c.Message,
        CreatedAt = c.CreatedAt
    };

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(idClaim!);
    }

    private static TicketResponseDto ToDto(SupportTicket t) => new()
    {
        TicketId = t.TicketId,
        Subject = t.Subject,
        Description = t.Description,
        Priority = t.Priority.ToString(),
        Status = t.Status.ToString(),
        Department = t.Department?.DepartmentName ?? "Unassigned",
        CreatedAt = t.CreatedAt
    };
}
