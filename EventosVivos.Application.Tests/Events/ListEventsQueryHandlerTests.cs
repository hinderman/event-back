using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Events.ListEvents;
using EventosVivos.Application.Tests.Common;
using EventosVivos.Domain.Catalogs;

namespace EventosVivos.Application.Tests.Events;

public sealed class ListEventsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NormalizesCriteriaAndCompletesPastEvents()
    {
        var readRepository = new FakeEventReadRepository
        {
            SearchResult = new PagedResponse<EventSummaryResponse>([TestData.EventSummary()], 2, 5, 11)
        };
        var eventRepository = new FakeEventRepository();
        var handler = new ListEventsQueryHandler(
            readRepository,
            eventRepository,
            new FakeCatalogRepository(),
            new TestDateTimeProvider(TestData.Now));

        var result = await handler.Handle(
            new ListEventsQuery(
                EventTypeCode: EventType.ConferenceCode,
                VenueId: 10,
                EventStatusCode: EventStatus.ActiveCode,
                Title: "  Clean  ",
                PageNumber: 2,
                PageSize: 5),
            CancellationToken.None);

        Assert.Equal(11, result.TotalItems);
        Assert.Equal("Clean", readRepository.LastCriteria?.Title);
        Assert.Equal(10, readRepository.LastCriteria?.VenueId?.Value);
        Assert.Equal(1, eventRepository.CompletePastActiveEventsCalls);
    }

    [Fact]
    public async Task Handle_WhenDateRangeIsInvalid_ThrowsApplicationRule()
    {
        var handler = new ListEventsQueryHandler(
            new FakeEventReadRepository(),
            new FakeEventRepository(),
            new FakeCatalogRepository(),
            new TestDateTimeProvider(TestData.Now));

        await Assert.ThrowsAsync<ApplicationRuleException>(() => handler.Handle(
            new ListEventsQuery(
                StartsFrom: new DateOnly(2026, 7, 2),
                StartsTo: new DateOnly(2026, 7, 1)),
            CancellationToken.None));
    }
}
