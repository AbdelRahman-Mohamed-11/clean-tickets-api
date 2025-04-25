using System.Security.Claims;
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
  ) : IRequestHandler<ListIncidentsQuery, Result<List<IncidentSummaryDto>>>
{
    public async Task<Result<List<IncidentSummaryDto>>> Handle(
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

        if (req.SupportStatus.HasValue) query = query.Where(i => i.SupportStatus == req.SupportStatus);
        if (req.UserStatus.HasValue) query = query.Where(i => i.UserStatus == req.UserStatus);
        if (req.Module.HasValue) query = query.Where(i => i.Module == req.Module);
        if (req.Priority.HasValue) query = query.Where(i => i.Priority == req.Priority);
        if (req.AssignedToId.HasValue) query = query.Where(i => i.AssignedToId == req.AssignedToId);
        if (req.FromDate.HasValue) query = query.Where(i => i.CreatedDate >= req.FromDate);
        if (req.ToDate.HasValue) query = query.Where(i => i.CreatedDate <= req.ToDate);

        var list = await query
          .Select(i => new IncidentSummaryDto(
            i.Id, i.CallType, i.Module, i.Priority, i.SupportStatus,
            i.UserStatus, i.Subject, i.CreatedDate, i.LoggedById, i.AssignedToId))
          .ToListAsync(ct);

        return Result.Success(list);
    }
}
