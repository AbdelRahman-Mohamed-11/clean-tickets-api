using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Core.Dtos.IncidentAttachments;

public class AddAttachmentsDto
{
    [Required]
    public List<IFormFile> Files { get; set; } = [];
}
