using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Auth.Login;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    ICatalogRepository catalogRepository,
    IPasswordHasher passwordHasher,
    IAuthTokenStore authTokenStore,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginCommand, AuthSessionResponse>
{
    public async Task<AuthSessionResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(new Email(request.Email), cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthUnauthorizedException("Correo o contrasena invalidos.");
        }

        var userType = await catalogRepository.GetUserTypeByIdAsync(user.UserTypeId, cancellationToken);

        if (userType is null)
        {
            throw new ApplicationRuleException("El tipo de usuario no esta configurado.");
        }

        var role = userType.Code.Value;
        var token = IssueToken(user, role);

        return AuthSessionResponse.Create(user, role, token);
    }

    private string IssueToken(AppUser user, string role)
    {
        return authTokenStore.IssueToken(new AuthenticatedUserSession(
            user.Id,
            user.FullName,
            user.Email.Value,
            role,
            dateTimeProvider.UtcNow));
    }
}
