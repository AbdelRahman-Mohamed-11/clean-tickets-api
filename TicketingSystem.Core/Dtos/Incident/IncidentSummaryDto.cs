using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Core.Dtos.Incident
{
    public record IncidentSummaryDto(
        Guid Id,                       
        DateTime CreatedDate,          
        DateTime? DeliveryDate,       
        string Description,           
        string Subject,                
        SupportStatus SupportStatus,   
        UserStatus UserStatus,        
        CallType CallType,            
        Priority Priority,             
        Module Module,                 
        Guid LoggedById,
        string? LoggedByUser,
        DateTime? ClosedDate,          
        DateTime StatusUpdatedDate,  
        string UrlOrFormName,      
        Guid? AssignedToId,
        string? AssignedToUserName,
        string CallRef
    );
}
