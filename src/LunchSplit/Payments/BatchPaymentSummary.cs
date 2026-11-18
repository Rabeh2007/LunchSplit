namespace LunchSplit.Payments;

/// <summary>Retains individual outcomes as well as the successful and failed counts.</summary>
public sealed record BatchPaymentSummary(IReadOnlyList<PaymentResult> Results)
{
    public int SuccessCount => Results.Count(result => result.IsSuccess);
    public int FailureCount => Results.Count - SuccessCount;
}
