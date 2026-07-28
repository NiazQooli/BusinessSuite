using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Privileges.Dtos;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Privileges.Queries.GetUserPrivileges;

public record GetUserPrivilegesQuery(byte CompanyId, string UserId)
    : IRequest<IReadOnlyList<UserPrivilegeDto>>;

public class GetUserPrivilegesQueryValidator : AbstractValidator<GetUserPrivilegesQuery>
{
    public GetUserPrivilegesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.")
            .MaximumLength(25);
    }
}

public class GetUserPrivilegesQueryHandler
    : IRequestHandler<GetUserPrivilegesQuery, IReadOnlyList<UserPrivilegeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserPrivilegesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<UserPrivilegeDto>> Handle(
        GetUserPrivilegesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId.Trim();

        return await _context.AdmUserPrivileges
            .AsNoTracking()
            .Where(p => p.CompanyId == request.CompanyId && p.UserId == userId)
            .OrderBy(p => p.MenuName)
            .Select(p => new UserPrivilegeDto(
                p.CompanyId,
                p.UserId,
                p.MenuName,
                p.HasRights,
                p.CanAdd,
                p.CanEdit,
                p.CanDelete,
                p.CanPost,
                p.CanPrint,
                p.CanView,
                p.CanExport,
                p.CanUnpost,
                p.CanCheck))
            .ToListAsync(cancellationToken);
    }
}
