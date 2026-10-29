namespace Kofi.Ordering.Application.Sagas;

public enum SubscriptionOrderStep
{
    ReservingStock,
    AwaitingPayment,
    Retrying,
    Compensating,
}
