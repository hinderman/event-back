using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.Users;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Tests.Users;

public sealed class AppUserTests
{
    [Fact]
    public void Create_BuildsActiveUserWithEmptyId()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);

        var user = AppUser.Create(
            new UserTypeId(2),
            "Compradora",
            new Email("buyer@eventos.com"),
            new PasswordHash("hash"),
            now);

        Assert.True(user.Id.IsEmpty);
        Assert.True(user.IsActive);
        Assert.Equal("buyer@eventos.com", user.Email.Value);
    }

    [Fact]
    public void ChangePasswordHash_ReplacesHashAndTouchesTimestamp()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);
        var user = NewUser(now);
        var updatedAt = now.AddHours(1);

        user.ChangePasswordHash(new PasswordHash("new-hash"), updatedAt);

        Assert.Equal("new-hash", user.PasswordHash.Value);
        Assert.Equal(updatedAt, user.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ChangesState()
    {
        var user = NewUser(new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero));

        user.Deactivate(user.UpdatedAt.AddHours(1));

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Constructor_RejectsBlankFullName()
    {
        var now = new DateTimeOffset(2026, 6, 19, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<DomainException>(() => new AppUser(
            new UserId(1),
            new UserTypeId(2),
            " ",
            new Email("buyer@eventos.com"),
            new PasswordHash("hash"),
            true,
            now,
            now));
    }

    private static AppUser NewUser(DateTimeOffset now)
    {
        return new AppUser(
            new UserId(1),
            new UserTypeId(2),
            "Compradora",
            new Email("buyer@eventos.com"),
            new PasswordHash("hash"),
            true,
            now,
            now);
    }
}
