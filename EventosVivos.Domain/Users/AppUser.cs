using EventosVivos.Domain.Primitives;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Users;

public sealed class AppUser : AggregateRoot<UserId>
{
    private AppUser()
    {
        FullName = string.Empty;
        Email = default;
        PasswordHash = default;
    }

    public AppUser(
        UserId id,
        UserTypeId userTypeId,
        string fullName,
        Email email,
        PasswordHash passwordHash,
        bool isActive,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        Guard.AgainstNonPositive(userTypeId.Value, nameof(userTypeId));

        UserTypeId = userTypeId;
        FullName = Guard.AgainstLength(Guard.AgainstBlank(fullName, nameof(fullName)), 150, nameof(fullName));
        Email = email;
        PasswordHash = passwordHash;
        IsActive = isActive;
        CreatedAt = Guard.AgainstDefault(createdAt, nameof(createdAt));
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }

    public UserTypeId UserTypeId { get; private set; }

    public string FullName { get; private set; }

    public Email Email { get; private set; }

    public PasswordHash PasswordHash { get; private set; }

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static AppUser Create(
        UserTypeId userTypeId,
        string fullName,
        Email email,
        PasswordHash passwordHash,
        DateTimeOffset now)
    {
        return new AppUser(UserId.Empty, userTypeId, fullName, email, passwordHash, true, now, now);
    }

    public void Activate(DateTimeOffset updatedAt)
    {
        IsActive = true;
        Touch(updatedAt);
    }

    public void Deactivate(DateTimeOffset updatedAt)
    {
        IsActive = false;
        Touch(updatedAt);
    }

    public void ChangePasswordHash(PasswordHash passwordHash, DateTimeOffset updatedAt)
    {
        PasswordHash = passwordHash;
        Touch(updatedAt);
    }

    private void Touch(DateTimeOffset updatedAt)
    {
        UpdatedAt = Guard.AgainstDefault(updatedAt, nameof(updatedAt));
    }
}
