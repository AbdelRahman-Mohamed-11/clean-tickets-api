using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.API.Controllers.RequestsDtos;

public class CreateIncidentDto
{
    public CallType CallType { get; set; }
    public Module Module { get; set; }
    public string UrlOrFormName { get; set; } = default!;
    public bool IsRecurring { get; set; }
    public Guid? RecurringCallId { get; set; }
    public Priority Priority { get; set; }
    public string Subject { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Suggestion { get; set; }
}
