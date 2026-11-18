namespace LunchSplit.Payments;

/// <summary>Boundary between LunchSplit payment logic and an external gateway.</summary>
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<PaymentStatus> GetTransactionStatusAsync(string transactionId);
}
