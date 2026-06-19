using EventosVivos.Application.Abstractions.Messaging;
using EventosVivos.Domain.Catalogs;

namespace EventosVivos.Application.Auth.Register;

public sealed record RegisterAccountCommand(
    string FullName,
    string Email,
    string Password,
    string UserTypeCode = UserType.BuyerCode) : ICommand<AuthSessionResponse>;
