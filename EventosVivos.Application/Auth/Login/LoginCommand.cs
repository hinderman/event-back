using EventosVivos.Application.Abstractions.Messaging;

namespace EventosVivos.Application.Auth.Login;

public sealed record LoginCommand(string Email, string Password) : ICommand<AuthSessionResponse>;
