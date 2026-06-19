using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Catalogs;

public sealed class UserType : CatalogItem<UserTypeId>
{
    public const string AdministratorCode = "administrador";
    public const string BuyerCode = "comprador";

    private UserType()
    {
    }

    public UserType(UserTypeId id, CatalogCode code, string name)
        : base(id, code, name)
    {
    }

    public bool IsAdministrator => Code.Value == AdministratorCode;

    public bool IsBuyer => Code.Value == BuyerCode;
}
