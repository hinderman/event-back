using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Common.Security;

public interface IUserAccessService
{
    Task<AppUser> EnsureActiveUserAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<AppUser> EnsureActiveAdministratorAsync(UserId userId, CancellationToken cancellationToken = default);

    Task<AppUser> EnsureActiveBuyerAsync(UserId userId, CancellationToken cancellationToken = default);

    Task EnsureCanCancelReservationAsync(
        AppUser user,
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
