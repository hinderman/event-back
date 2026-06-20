namespace EventosVivos.Api.Endpoints.Auth.Requests;

public sealed record RegisterAccountRequest(
    string FullName,
    string Email,
    string Password,
    string? UserTypeCode);
