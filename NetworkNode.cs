using System.ComponentModel.DataAnnotations;

namespace ISP_Portal.API.Models.Dtos;

public class CreateCommentDto
{
    [Required]
    public string Message { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int CommentId { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateTicketStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
