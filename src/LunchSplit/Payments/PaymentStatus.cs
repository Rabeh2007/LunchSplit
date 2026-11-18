namespace LunchSplit.Payments;

/// <summary>Transaction states returned by the payment gateway.</summary>
public enum PaymentStatus
{
    UNKNOWN,
    APPROVED,
    PENDING,
    COMPLETED,
    CANCELED,
    FAILED
}
