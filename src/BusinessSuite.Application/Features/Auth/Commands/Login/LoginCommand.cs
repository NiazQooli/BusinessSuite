using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Application.Features.Auth.Dtos;
using BusinessSuite.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Features.Auth.Commands.Login;

public record LoginCommand(string UserName, string Password) : IRequest<LoginResponseDto>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(100);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userName = request.UserName.Trim();
        var password = request.Password.Trim();

        // Plain-text compare against legacy tblAdmUser (UserID + Password).
        var matches = await _context.AdmUsers
            .AsNoTracking()
            .Where(u => u.UserId == userName && u.Password == password)
            .OrderBy(u => u.CompanyId)
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            throw new UnauthorizedException("Invalid user name or password.");
        }

        var user = matches.FirstOrDefault(u => u.Enabled);
        if (user is null)
        {
            throw new UnauthorizedException("User account is disabled.");
        }

        var displayName = string.IsNullOrWhiteSpace(user.UserName) ? user.UserId : user.UserName;
        var subject = $"{user.CompanyId}:{user.UserId}";
        var token = _jwtTokenService.CreateToken(subject, user.UserId, displayName, user.CompanyId);

        return new LoginResponseDto(
            token,
            _jwtTokenService.GetExpiryUtc(),
            user.UserId,
            user.CompanyId, // tinyint → int for API response
            user.UserId,
            displayName);
    }
}
