using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Core.Entities;

public class IncidentAttachment
{
    public Guid Id { get; private set; }
    public Guid IncidentId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string FilePath { get; private set; } = default!;
    public Guid UploaderId { get; private set; }
    public DateTime UploadedAt { get; private set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public Incident Incident { get; private set; } = default!;
    public ApplicationUser Uploader { get; private set; } = default!;


    public IncidentAttachment(Guid incidentId, string fileName, string filePath, Guid uploaderId)
    {
        IncidentId = incidentId;
        FileName = fileName;
        FilePath = filePath;
        UploaderId = uploaderId;
    }
}
