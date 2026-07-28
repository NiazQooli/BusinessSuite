using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Menus.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Menus.Queries.GetMenuItems;

public record GetMenuItemsQuery(byte CompanyId, bool IncludeInactive = false)
    : IRequest<IReadOnlyList<MenuItemDto>>;

public class GetMenuItemsQueryHandler
    : IRequestHandler<GetMenuItemsQuery, IReadOnlyList<MenuItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMenuItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<MenuItemDto>> Handle(
        GetMenuItemsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.SftMenuItems
            .AsNoTracking()
            .Where(m => m.CompanyId == request.CompanyId);

        if (!request.IncludeInactive)
        {
            query = query.Where(m => !m.InactiveMenu);
        }

        return await query
            .OrderBy(m => m.MenuPriority)
            .ThenBy(m => m.MenuSrNo)
            .ThenBy(m => m.MenuCaption)
            .Select(m => new MenuItemDto(
                m.CompanyId,
                m.MenuName,
                m.MenuSrNo,
                m.MainMenuName,
                m.MenuCaption,
                m.HasRights,
                m.MenuLevel,
                m.CompleteMenuName,
                m.ModuleName,
                m.IsReportActive,
                m.MenuPriority,
                m.InactiveMenu))
            .ToListAsync(cancellationToken);
    }
}
