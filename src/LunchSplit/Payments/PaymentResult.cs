namespace LunchSplit.Payments;

/// <summary>A gateway outcome without any dependency on Square SDK types.</summary>
public sealed record PaymentResult(bool IsSuccess, string? TransactionId = null,
    string? ErrorCode = null, int? HttpStatusCode = null);
