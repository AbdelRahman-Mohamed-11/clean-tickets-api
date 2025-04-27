using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Application.Utilities;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Entities.Identity.Enums;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.Paged;

public class PagedIncidentsQueryHandler(
    ITicketDbContext db,
    IHttpContextAccessor http,
    ILogger<PagedIncidentsQueryHandler> logger
) : IRequestHandler<PagedIncidentsQuery, Result<PagedList<IncidentSummaryDto>>>
{
    public async Task<Result<PagedList<IncidentSummaryDto>>> Handle(
      PagedIncidentsQuery req,
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

        var totalCount = await query.CountAsync(ct);

        var data = await query
          .OrderByDescending(i => i.CreatedDate)
          .Skip((req.PageNumber - 1) * req.PageSize)
          .Take(req.PageSize)
          .Select(i => new IncidentSummaryDto(
              i.Id,
              i.CreatedDate,
              i.DeliveryDate,
              i.Description,
              i.Subject,
              i.SupportStatus,
              i.UserStatus,
              i.CallType,
              i.Priority,
              i.Module,
              i.LoggedById,
              i.LoggedBy != null ? i.LoggedBy.UserName : "",
              i.ClosedDate,
              i.StatusUpdatedDate,
              i.UrlOrFormName,
              i.AssignedToId,
              i.AssignedTo != null ? i.AssignedTo.UserName : "",
              i.CallRef
          ))
          .ToListAsync(ct);

        var pagedList = new PagedList<IncidentSummaryDto>(
            data,
            req.PageNumber,
            req.PageSize,
            totalCount
        );

        return Result.Success(pagedList);
    }
}