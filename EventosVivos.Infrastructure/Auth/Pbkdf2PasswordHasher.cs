using System.Security.Cryptography;
using EventosVivos.Application.Abstractions.Auth;
using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Infrastructure.Auth;

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;
    private const string Algorithm = "pbkdf2-sha256";

    public PasswordHash Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = DeriveKey(password, salt, Iterations);
        var encoded = string.Join(
            "$",
            Algorithm,
            Iterations.ToString(),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));

        return new PasswordHash(encoded);
    }

    public bool Verify(string password, PasswordHash passwordHash)
    {
        var parts = passwordHash.Value.Split('$');

        if (parts.Length != 4 || parts[0] != Algorithm || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedKey = Convert.FromBase64String(parts[3]);
            var actualKey = DeriveKey(password, salt, iterations);

            return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static byte[] DeriveKey(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            KeySize);
    }
}
