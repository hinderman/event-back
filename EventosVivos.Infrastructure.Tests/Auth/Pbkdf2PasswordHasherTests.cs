using EventosVivos.Domain.ValueObjects;
using EventosVivos.Infrastructure.Auth;

namespace EventosVivos.Infrastructure.Tests.Auth;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Hash_ProducesPbkdf2HashThatVerifies()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var hash = hasher.Hash("Password123");

        Assert.StartsWith("pbkdf2-sha256$210000$", hash.Value);
        Assert.True(hasher.Verify("Password123", hash));
    }

    [Fact]
    public void Hash_UsesDifferentSaltForSamePassword()
    {
        var hasher = new Pbkdf2PasswordHasher();

        var first = hasher.Hash("Password123");
        var second = hasher.Hash("Password123");

        Assert.NotEqual(first, second);
        Assert.True(hasher.Verify("Password123", first));
        Assert.True(hasher.Verify("Password123", second));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("Password123");

        Assert.False(hasher.Verify("Password124", hash));
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("pbkdf2-sha256$iterations$salt$key")]
    [InlineData("other$210000$salt$key")]
    public void Verify_ReturnsFalseForMalformedHash(string value)
    {
        var hasher = new Pbkdf2PasswordHasher();

        Assert.False(hasher.Verify("Password123", new PasswordHash(value)));
    }
}
