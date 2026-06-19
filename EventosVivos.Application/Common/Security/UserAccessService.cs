using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Common.Security;

internal sealed class UserAccessService(
    IUserRepository userRepository,
    ICatalogRepository catalogRepository)
    : IUserAccessService
{
    public async Task<AppUser> EnsureActiveUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Usuario", userId.Value);
        }

        if (!user.IsActive)
        {
            throw new ForbiddenAccessException("El usuario no esta activo.");
        }

        return user;
    }

    public async Task<AppUser> EnsureActiveAdministratorAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var user = await EnsureActiveUserAsync(userId, cancellationToken);
        var userType = await GetUserTypeAsync(user.UserTypeId, cancellationToken);

        if (!userType.IsAdministrator)
        {
            throw new ForbiddenAccessException("La operacion requiere un usuario administrador.");
        }

        return user;
    }

    public async Task<AppUser> EnsureActiveBuyerAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await EnsureActiveUserAsync(userId, cancellationToken);
        var userType = await GetUserTypeAsync(user.UserTypeId, cancellationToken);

        if (!userType.IsBuyer)
        {
            throw new ForbiddenAccessException("La operacion requiere un usuario comprador.");
        }

        return user;
    }

    public async Task EnsureCanCancelReservationAsync(
        AppUser user,
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var userType = await GetUserTypeAsync(user.UserTypeId, cancellationToken);

        if (userType.IsAdministrator)
        {
            return;
        }

        if (userType.IsBuyer && reservation.BuyerUserId == user.Id)
        {
            return;
        }

        throw new ForbiddenAccessException("Solo el comprador de la reserva o un administrador puede cancelarla.");
    }

    private async Task<UserType> GetUserTypeAsync(UserTypeId userTypeId, CancellationToken cancellationToken)
    {
        var userType = await catalogRepository.GetUserTypeByIdAsync(userTypeId, cancellationToken);

        return userType ?? throw new ApplicationRuleException("El tipo de usuario no esta configurado.");
    }
}
