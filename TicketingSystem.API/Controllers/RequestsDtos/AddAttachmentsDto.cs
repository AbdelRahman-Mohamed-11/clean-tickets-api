using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.API.Controllers.RequestsDtos;

public class AddAttachmentsDto
{
    [Required]
    public List<IFormFile> Files { get; set; } = [];
}
