using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Common.Pagination;
using EventosVivos.Application.Reservations.ListReservations;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Reservations;

public sealed class ListReservationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NormalizesCriteriaAndReturnsPagedResult()
    {
        var repository = new FakeReservationReadRepository
        {
            SearchResult = new PagedResponse<ReservationListItemResponse>([], 1, 10, 0)
        };
        var handler = new ListReservationsQueryHandler(repository);

        var result = await handler.Handle(
            new ListReservationsQuery(PageNumber: 2, PageSize: 20, SortBy: " buyerName ", SortDirection: " ASC "),
            CancellationToken.None);

        Assert.Equal(0, result.TotalItems);
        Assert.Equal(2, repository.LastCriteria?.PageNumber);
        Assert.Equal(20, repository.LastCriteria?.PageSize);
        Assert.Equal("buyerName", repository.LastCriteria?.SortBy);
        Assert.Equal("asc", repository.LastCriteria?.SortDirection);
    }

    [Fact]
    public async Task Handle_WhenSortFieldIsInvalid_ThrowsApplicationRule()
    {
        var handler = new ListReservationsQueryHandler(new FakeReservationReadRepository());

        await Assert.ThrowsAsync<ApplicationRuleException>(() => handler.Handle(
            new ListReservationsQuery(SortBy: "invalid"),
            CancellationToken.None));
    }
}
