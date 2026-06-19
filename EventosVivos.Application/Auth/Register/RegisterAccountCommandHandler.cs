using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Auth.Register;

public sealed class RegisterAccountCommandHandler(
    IUserRepository userRepository,
    ICatalogRepository catalogRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAuthTokenStore authTokenStore,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterAccountCommand, AuthSessionResponse>
{
    public async Task<AuthSessionResponse> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        EnsureValidPassword(request.Password);

        var email = new Email(request.Email);
        var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException("Ya existe una cuenta registrada con ese correo.");
        }

        var userType = await GetUserTypeAsync(request.UserTypeCode, cancellationToken);
        var now = dateTimeProvider.UtcNow;
        var user = AppUser.Create(
            userType.Id,
            request.FullName,
            email,
            passwordHasher.Hash(request.Password),
            now);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var token = IssueToken(user, userType.Code.Value, now);
        return AuthSessionResponse.Create(user, userType.Code.Value, token);
    }

    private async Task<UserType> GetUserTypeAsync(string userTypeCode, CancellationToken cancellationToken)
    {
        var code = new CatalogCode(userTypeCode);
        var userType = await catalogRepository.GetUserTypeByCodeAsync(code, cancellationToken);

        if (userType is null)
        {
            throw new ApplicationRuleException($"El tipo de usuario '{code.Value}' no esta configurado.");
        }

        if (!userType.IsAdministrator && !userType.IsBuyer)
        {
            throw new ApplicationRuleException("El tipo de usuario no es valido para autenticacion.");
        }

        return userType;
    }

    private static void EnsureValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8 || password.Length > 100)
        {
            throw new ApplicationRuleException("La contrasena debe tener entre 8 y 100 caracteres.");
        }
    }

    private string IssueToken(AppUser user, string role, DateTimeOffset now)
    {
        return authTokenStore.IssueToken(new AuthenticatedUserSession(
            user.Id,
            user.FullName,
            user.Email.Value,
            role,
            now));
    }
}
