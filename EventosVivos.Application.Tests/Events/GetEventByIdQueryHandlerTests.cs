using EventosVivos.Application.Common.Errors;
using EventosVivos.Application.Events.GetEventById;
using EventosVivos.Application.Events.GetEventById.Handlers;
using EventosVivos.Application.Tests.Common;

namespace EventosVivos.Application.Tests.Events;

public sealed class GetEventByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsEvent_WhenFound()
    {
        var readRepository = new FakeEventReadRepository { EventById = TestData.EventSummary(99) };
        var handler = new GetEventByIdQueryHandler(readRepository);

        var result = await handler.Handle(new GetEventByIdQuery(99), CancellationToken.None);

        Assert.Equal(99, result.Id);
    }

    [Fact]
    public async Task Handle_WhenEventDoesNotExist_ThrowsNotFound()
    {
        var handler = new GetEventByIdQueryHandler(new FakeEventReadRepository());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetEventByIdQuery(99), CancellationToken.None));
    }
}
