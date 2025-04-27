using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Entities.Identity.Enums;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.List;


public class ListIncidentsQueryHandler(
    ITicketDbContext db,
    IHttpContextAccessor http,
    ILogger<ListIncidentsQueryHandler> logger
) : IRequestHandler<ListIncidentsQuery, Result<List<IncidentListDto>>>
{
    public async Task<Result<List<IncidentListDto>>> Handle(
      ListIncidentsQuery req,
      CancellationToken ct)
    {
        logger.LogInformation("Listing incidents with filters {@Filters}", req);

        var user = http.HttpContext!.User;
        var isAdmin = user.IsInRole(nameof(Role.Admin));
        var isErp = user.IsInRole(nameof(Role.ERP));

        var query = db.Incidents.AsNoTracking();

        if (!isAdmin && !isErp)
        {
            var me = user.FindFirst("ID")!.Value;
            query = query.Where(i => i.LoggedById == Guid.Parse(me));
        }

     
        var data = await query
          .OrderByDescending(i => i.CreatedDate)
          .Select(i => new IncidentListDto(
              i.Id,
              i.Subject,
              i.CallRef
          ))
          .ToListAsync(ct);


        return Result.Success(data);
    }
}