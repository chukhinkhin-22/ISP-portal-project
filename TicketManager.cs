using ISP_Portal.API.Models;
using ISP_Portal.API.Models.Dtos;

namespace ISP_Portal.API.Services;

public interface ITicketManager
{
    Task<SupportTicket> SubmitTicketAsync(int userId, CreateTicketDto dto);
}
