using EventosVivos.Domain.ValueObjects;

namespace EventosVivos.Domain.Catalogs;

public sealed class ReservationStatus : CatalogItem<ReservationStatusId>
{
    public const string PendingPaymentCode = "pendiente_pago";
    public const string ConfirmedCode = "confirmada";
    public const string CanceledCode = "cancelada";

    private ReservationStatus()
    {
    }

    public ReservationStatus(ReservationStatusId id, CatalogCode code, string name)
        : base(id, code, name)
    {
    }
}
