using EventosVivos.Application.Abstractions.Clock;
using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Application.Abstractions.Persistence;
using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Security;
using EventosVivos.Application.Events;
using EventosVivos.Domain.Catalogs;
using EventosVivos.Domain.Events;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Application.Events.CreateEvent;

public sealed class CreateEventCommandHandler(
    IEventRepository eventRepository,
    IVenueRepository venueRepository,
    ICatalogRepository catalogRepository,
    IUserAccessService userAccessService,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateEventCommand, EventResponse>
{
    public async Task<EventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var createdByUserId = new UserId(request.CreatedByUserId);
        var createdBy = await userAccessService.EnsureActiveAdministratorAsync(createdByUserId, cancellationToken);

        var venueId = new VenueId(request.VenueId);
        var venue = await venueRepository.GetByIdAsync(venueId, cancellationToken);

        if (venue is null)
        {
            throw new NotFoundException("Venue", venueId.Value);
        }

        if (!venue.IsActive)
        {
            throw new ConflictException("El venue no esta activo.");
        }

        var eventTypeCode = new CatalogCode(request.EventTypeCode);
        var eventType = await catalogRepository.GetEventTypeByCodeAsync(eventTypeCode, cancellationToken);

        if (eventType is null)
        {
            throw new NotFoundException("Tipo de evento", eventTypeCode.Value);
        }

        var activeStatus = await GetEventStatusAsync(EventStatus.ActiveCode, cancellationToken);
        var details = new EventText(request.Title, request.Description);
        var maxCapacity = new Capacity(request.MaxCapacity);
        var schedule = new EventSchedule(request.StartsAt, request.EndsAt);
        var price = new Money(request.Price);
        var now = dateTimeProvider.UtcNow;

        if (!venue.CanHost(maxCapacity))
        {
            throw new ConflictException("La capacidad del evento no puede superar la capacidad del venue.");
        }

        var overlaps = await eventRepository.ExistsActiveOverlapAsync(
            venue.Id,
            schedule,
            activeStatus.Id,
            cancellationToken);

        if (overlaps)
        {
            throw new ConflictException("Ya existe un evento activo solapado en el mismo venue.");
        }

        var @event = Event.Create(
            venue.Id,
            eventType.Id,
            activeStatus.Id,
            createdBy.Id,
            details,
            maxCapacity,
            schedule,
            price,
            now);

        await eventRepository.AddAsync(@event, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return EventResponse.FromEvent(@event);
    }

    private async Task<EventStatus> GetEventStatusAsync(string code, CancellationToken cancellationToken)
    {
        var statusCode = new CatalogCode(code);
        var status = await catalogRepository.GetEventStatusByCodeAsync(statusCode, cancellationToken);

        return status ?? throw new ApplicationRuleException($"El estado de evento '{code}' no esta configurado.");
    }
}
