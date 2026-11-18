namespace LunchSplit.Payments;

/// <summary>Wraps transport failures so callers need not depend on HTTP exceptions.</summary>
public sealed class PaymentGatewayTimeoutException : Exception
{
    public PaymentGatewayTimeoutException(string message, Exception innerException)
        : base(message, innerException) { }
}
