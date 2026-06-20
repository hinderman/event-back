using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Abstractions.Reservations;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Common.Security;
using EventosVivos.Application.Events.ListEvents;
using EventosVivos.Application.Reports;
using EventosVivos.Application.Reservations.ListReservations;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.Reservations;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;
using EventosVivos.Domain.Venues;

namespace EventosVivos.Application.Tests.Common;

internal sealed class TestDateTimeProvider(DateTimeOffset utcNow) : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = utcNow;
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCalls { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCalls++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string ValidPassword { get; set; } = "Password123";

    public PasswordHash Hash(string password)
    {
        return new PasswordHash($"hashed:{password}");
    }

    public bool Verify(string password, PasswordHash passwordHash)
    {
        return password == ValidPassword && passwordHash.Value == $"hashed:{ValidPassword}";
    }
}

internal sealed class FakeAuthTokenStore : IAuthTokenStore
{
    private int sequence;
    private readonly Dictionary<string, AuthenticatedUserSession> sessions = new();

    public IReadOnlyDictionary<string, AuthenticatedUserSession> Sessions => sessions;

    public string IssueToken(AuthenticatedUserSession session)
    {
        var token = $"token-{++sequence}";
        sessions[token] = session;
        return token;
    }

    public bool TryGetSession(string token, out AuthenticatedUserSession session)
    {
        return sessions.TryGetValue(token, out session!);
    }

    public bool RevokeToken(string token)
    {
        return sessions.Remove(token);
    }
}

internal sealed class FakeUserRepository : IUserRepository
{
    private readonly Dictionary<long, AppUser> byId = new();
    private readonly Dictionary<string, AppUser> byEmail = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<AppUser> Users => byId.Values;

    public Task<AppUser?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(byId.GetValueOrDefault(userId.Value));
    }

    public Task<AppUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(byEmail.GetValueOrDefault(email.Value));
    }

    public Task AddAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var persisted = user.Id.IsEmpty
            ? TestData.User(
                byId.Count + 1,
                user.UserTypeId,
                user.FullName,
                user.Email.Value,
                user.PasswordHash.Value,
                user.IsActive,
                user.CreatedAt)
            : user;

        Add(persisted);
        return Task.CompletedTask;
    }

    public void Add(AppUser user)
    {
        byId[user.Id.Value] = user;
        byEmail[user.Email.Value] = user;
    }
}

internal sealed class FakeCatalogRepository : ICatalogRepository
{
    private readonly Dictionary<short, UserType> userTypesById = new();
    private readonly Dictionary<string, UserType> userTypesByCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, EventType> eventTypesByCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, EventStatus> eventStatusesByCode = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ReservationStatus> reservationStatusesByCode = new(StringComparer.OrdinalIgnoreCase);

    public FakeCatalogRepository()
    {
        AddUserType(new UserType(new UserTypeId(1), new CatalogCode(UserType.AdministratorCode), "Administrador"));
        AddUserType(new UserType(new UserTypeId(2), new CatalogCode(UserType.BuyerCode), "Comprador"));
        AddEventType(new EventType(new EventTypeId(1), new CatalogCode(EventType.ConferenceCode), "Conferencia"));
        AddEventStatus(new EventStatus(new EventStatusId(1), new CatalogCode(EventStatus.ActiveCode), "Activo"));
        AddEventStatus(new EventStatus(new EventStatusId(2), new CatalogCode(EventStatus.CanceledCode), "Cancelado"));
        AddEventStatus(new EventStatus(new EventStatusId(3), new CatalogCode(EventStatus.CompletedCode), "Completado"));
        AddReservationStatus(new ReservationStatus(new ReservationStatusId(1), new CatalogCode(ReservationStatus.PendingPaymentCode), "Pendiente"));
        AddReservationStatus(new ReservationStatus(new ReservationStatusId(2), new CatalogCode(ReservationStatus.ConfirmedCode), "Confirmada"));
        AddReservationStatus(new ReservationStatus(new ReservationStatusId(3), new CatalogCode(ReservationStatus.CanceledCode), "Cancelada"));
    }

    public Task<UserType?> GetUserTypeByIdAsync(UserTypeId userTypeId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(userTypesById.GetValueOrDefault(userTypeId.Value));
    }

    public Task<UserType?> GetUserTypeByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(userTypesByCode.GetValueOrDefault(code.Value));
    }

    public Task<EventType?> GetEventTypeByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(eventTypesByCode.GetValueOrDefault(code.Value));
    }

    public Task<EventStatus?> GetEventStatusByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(eventStatusesByCode.GetValueOrDefault(code.Value));
    }

    public Task<ReservationStatus?> GetReservationStatusByCodeAsync(CatalogCode code, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(reservationStatusesByCode.GetValueOrDefault(code.Value));
    }

    private void AddUserType(UserType userType)
    {
        userTypesById[userType.Id.Value] = userType;
        userTypesByCode[userType.Code.Value] = userType;
    }

    private void AddEventType(EventType eventType)
    {
        eventTypesByCode[eventType.Code.Value] = eventType;
    }

    private void AddEventStatus(EventStatus status)
    {
        eventStatusesByCode[status.Code.Value] = status;
    }

    private void AddReservationStatus(ReservationStatus status)
    {
        reservationStatusesByCode[status.Code.Value] = status;
    }
}

internal sealed class FakeVenueRepository : IVenueRepository
{
    private readonly Dictionary<int, Venue> venues = new();

    public Task<Venue?> GetByIdAsync(VenueId venueId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(venues.GetValueOrDefault(venueId.Value));
    }

    public void Add(Venue venue)
    {
        venues[venue.Id.Value] = venue;
    }
}

internal sealed class FakeEventRepository : IEventRepository
{
    private readonly Dictionary<long, Event> events = new();

    public bool HasActiveOverlap { get; set; }
    public int CompletePastActiveEventsCalls { get; private set; }
    public EventSchedule? LastOverlapSchedule { get; private set; }
    public Event? AddedEvent { get; private set; }

    public Task<Event?> GetByIdAsync(EventId eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(events.GetValueOrDefault(eventId.Value));
    }

    public Task<bool> ExistsActiveOverlapAsync(
        VenueId venueId,
        EventSchedule schedule,
        EventStatusId activeStatusId,
        CancellationToken cancellationToken = default)
    {
        LastOverlapSchedule = schedule;
        return Task.FromResult(HasActiveOverlap);
    }

    public Task<int> CompletePastActiveEventsAsync(
        EventStatusId activeStatusId,
        EventStatusId completedStatusId,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        CompletePastActiveEventsCalls++;
        return Task.FromResult(0);
    }

    public Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        AddedEvent = @event;
        return Task.CompletedTask;
    }

    public void Add(Event @event)
    {
        events[@event.Id.Value] = @event;
    }
}

internal sealed class FakeReservationRepository : IReservationRepository
{
    private readonly Dictionary<long, Reservation> reservations = new();

    public int ReservedTickets { get; set; }
    public Reservation? AddedReservation { get; private set; }

    public Task<Reservation?> GetByIdAsync(ReservationId reservationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(reservations.GetValueOrDefault(reservationId.Value));
    }

    public Task<int> CountReservedTicketsByEventIdAsync(
        EventId eventId,
        ReservationStatusId canceledStatusId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ReservedTickets);
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        AddedReservation = reservation;
        return Task.CompletedTask;
    }

    public void Add(Reservation reservation)
    {
        reservations[reservation.Id.Value] = reservation;
    }
}

internal sealed class FakeUserAccessService : IUserAccessService
{
    private readonly FakeUserRepository users;
    private readonly FakeCatalogRepository catalogs;

    public FakeUserAccessService(FakeUserRepository users, FakeCatalogRepository catalogs)
    {
        this.users = users;
        this.catalogs = catalogs;
    }

    public Task<AppUser> EnsureActiveUserAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return GetActiveUserAsync(userId, cancellationToken);
    }

    public async Task<AppUser> EnsureActiveAdministratorAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);
        var type = await catalogs.GetUserTypeByIdAsync(user.UserTypeId, cancellationToken);
        if (type?.IsAdministrator != true)
        {
            throw new EventosVivos.Application.Common.Errors.ForbiddenAccessException("La operacion requiere un usuario administrador.");
        }

        return user;
    }

    public async Task<AppUser> EnsureActiveBuyerAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var user = await GetActiveUserAsync(userId, cancellationToken);
        var type = await catalogs.GetUserTypeByIdAsync(user.UserTypeId, cancellationToken);
        if (type?.IsBuyer != true)
        {
            throw new EventosVivos.Application.Common.Errors.ForbiddenAccessException("La operacion requiere un usuario comprador.");
        }

        return user;
    }

    public async Task EnsureCanCancelReservationAsync(AppUser user, Reservation reservation, CancellationToken cancellationToken = default)
    {
        var type = await catalogs.GetUserTypeByIdAsync(user.UserTypeId, cancellationToken);
        if (type?.IsAdministrator == true || (type?.IsBuyer == true && reservation.BuyerUserId == user.Id))
        {
            return;
        }

        throw new EventosVivos.Application.Common.Errors.ForbiddenAccessException("Solo el comprador de la reserva o un administrador puede cancelarla.");
    }

    private async Task<AppUser> GetActiveUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new EventosVivos.Application.Common.Errors.NotFoundException("Usuario", userId.Value);

        if (!user.IsActive)
        {
            throw new EventosVivos.Application.Common.Errors.ForbiddenAccessException("El usuario no esta activo.");
        }

        return user;
    }
}

internal sealed class FakeReservationCodeGenerator : IReservationCodeGenerator
{
    public ReservationCode NextCode { get; set; } = new("EV-000123");

    public Task<ReservationCode> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(NextCode);
    }
}

internal sealed class FakeEventReadRepository : IEventReadRepository
{
    public EventSearchCriteria? LastCriteria { get; private set; }
    public EventSummaryResponse? EventById { get; set; }
    public PagedResponse<EventSummaryResponse> SearchResult { get; set; } = new([], 1, 10, 0);

    public Task<PagedResponse<EventSummaryResponse>> SearchAsync(EventSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;
        return Task.FromResult(SearchResult);
    }

    public Task<EventSummaryResponse?> GetByIdAsync(long eventId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(EventById);
    }
}

internal sealed class FakeReservationReadRepository : IReservationReadRepository
{
    public ReservationSearchCriteria? LastCriteria { get; private set; }
    public PagedResponse<ReservationListItemResponse> SearchResult { get; set; } = new([], 1, 10, 0);

    public Task<PagedResponse<ReservationListItemResponse>> SearchAsync(
        ReservationSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        LastCriteria = criteria;
        return Task.FromResult(SearchResult);
    }
}

internal sealed class UnusedReportingRepository : IReportingRepository
{
    public Task<IReadOnlyCollection<EventOccupancyReportResponse>> GetEventOccupancyAsync(
        EventId? eventId = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<EventOccupancyReportResponse>>([]);
    }
}

internal static class TestData
{
    public static readonly DateTimeOffset Now = new(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);

    public static AppUser Admin(long id = 1)
    {
        return User(id, new UserTypeId(1), "Admin User", "admin@eventos.com", "hashed:Password123", true, Now);
    }

    public static AppUser Buyer(long id = 2, string email = "buyer@eventos.com")
    {
        return User(id, new UserTypeId(2), "Buyer User", email, "hashed:Password123", true, Now);
    }

    public static AppUser User(
        long id,
        UserTypeId userTypeId,
        string fullName,
        string email,
        string passwordHash,
        bool isActive,
        DateTimeOffset now)
    {
        return new AppUser(
            new UserId(id),
            userTypeId,
            fullName,
            new Email(email),
            new PasswordHash(passwordHash),
            isActive,
            now,
            now);
    }

    public static Venue Venue(int id = 10, int capacity = 100, bool active = true)
    {
        return new Venue(
            new VenueId(id),
            "Auditorio Principal",
            "Bogota",
            new Capacity(capacity),
            active,
            Now,
            Now);
    }

    public static Event ActiveEvent(
        long id = 20,
        int capacity = 100,
        decimal price = 80m,
        DateTimeOffset? startsAt = null)
    {
        var start = startsAt ?? Now.AddDays(5);
        return new Event(
            new EventId(id),
            new VenueId(10),
            new EventTypeId(1),
            new EventStatusId(1),
            new UserId(1),
            new EventText("Conferencia Clean", "Aprendizajes de arquitectura limpia"),
            new Capacity(capacity),
            new EventSchedule(start, start.AddHours(2)),
            new Money(price),
            Now,
            Now);
    }

    public static Reservation Reservation(
        long id = 30,
        short statusId = 1,
        long eventId = 20,
        long buyerId = 2,
        int quantity = 2,
        decimal unitPrice = 80m,
        DateTimeOffset? now = null)
    {
        var timestamp = now ?? Now;
        return new Reservation(
            new ReservationId(id),
            new EventId(eventId),
            new UserId(buyerId),
            new ReservationStatusId(statusId),
            new TicketQuantity(quantity),
            new Money(unitPrice),
            null,
            null,
            null,
            null,
            null,
            false,
            null,
            null,
            null,
            timestamp,
            timestamp);
    }

    public static EventSummaryResponse EventSummary(long id = 20)
    {
        return new EventSummaryResponse(
            id,
            "Conferencia Clean",
            "Aprendizajes de arquitectura limpia",
            10,
            "Auditorio Principal",
            "Bogota",
            EventType.ConferenceCode,
            "Conferencia",
            EventStatus.ActiveCode,
            "Activo",
            100,
            Now.AddDays(5),
            Now.AddDays(5).AddHours(2),
            80m,
            10,
            90);
    }
}
