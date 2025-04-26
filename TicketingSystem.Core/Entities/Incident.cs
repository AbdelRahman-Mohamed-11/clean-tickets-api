using TicketingSystem.Core.Entities.Identity.Enums;
using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Core.Entities;

public class Incident(
       Guid loggedById,
       CallType callType,
       Module module,
       string urlOrFormName,
       bool isRecurring,
       Guid? recurringCallId,
       Priority priority,
       string subject,
       string description,
       string? suggestion = null
       )
{
    public Guid Id { get; private set; }
    public Guid LoggedById { get; private set; } = loggedById;
    public Guid? AssignedToId { get; private set; }
    public CallType CallType { get; private set; } = callType;
    public Module Module { get; private set; } = module;
    public string UrlOrFormName { get; private set; } = urlOrFormName;
    public bool IsRecurring { get; private set; } = isRecurring;
    public bool IsDeleted { get; set; } = false;

    public Guid? RecurringCallId { get; private set; } = recurringCallId;
    public Priority Priority { get; private set; } = priority;
    public string Subject { get; private set; } = subject;
    public string Description { get; private set; } = description;
    public string? Suggestion { get; private set; } = suggestion;
    public SupportStatus SupportStatus { get; private set; } = SupportStatus.Pending;
    public UserStatus UserStatus { get; private set; } = UserStatus.Pending;
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? DeliveryDate { get; private set; }
    public DateTime StatusUpdatedDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ClosedDate { get; private set; }
    
    public ApplicationUser LoggedBy { get; private set; } = default!;
    public ApplicationUser? AssignedTo { get; private set; }
    public Incident? RecurringCall { get; private set; }

    private readonly List<IncidentComment> _comments = new();
    private readonly List<IncidentAttachment> _attachments = new();
    public IReadOnlyCollection<IncidentComment> Comments => _comments;
    public IReadOnlyCollection<IncidentAttachment> Attachments => _attachments;


    public void SetSuggestion(string? suggestion)
    {
        Suggestion = suggestion;
    }

    public void SetSupportStatus(SupportStatus supportStatus)
    {
        SupportStatus = supportStatus;
    }

    public void SetUserStatus(UserStatus userStatus)
    {
        UserStatus = userStatus;
    }

    public void SetAssignedTo(Guid? assignedToId)
    {
        AssignedToId = assignedToId;
    }


    public void SetDeliveryDate(DateTime deliveryDate)
    {
        DeliveryDate = deliveryDate;
    }

    public void SetClosedDate(DateTime closedDate)
    {
        ClosedDate = closedDate;
    }

    public void SetStatusUpdatedDate(DateTime statusUpdatedDate)
    {
        StatusUpdatedDate = statusUpdatedDate;
    }

}
