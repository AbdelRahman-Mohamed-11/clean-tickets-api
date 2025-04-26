using TicketingSystem.API.Controllers.RequestsDtos;
using TicketingSystem.Application.incidents.Create;

namespace TicketingSystem.API.Extensions
{
    public static class IncidentMapper
    {
        public static CreateIncidentCommand ToCommand(CreateIncidentDto dto)
        {
            return new CreateIncidentCommand(
                dto.CallType,
                dto.Module,
                dto.UrlOrFormName,
                dto.IsRecurring,
                dto.RecurringCallId,
                dto.Priority,
                dto.Subject,
                dto.Description,
                dto.Suggestion);
        }
    }
}
