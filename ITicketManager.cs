namespace ISP_Portal.API.Services;

public interface ITicketRouter
{
    RoutingResult Route(string subject, string description);
}
