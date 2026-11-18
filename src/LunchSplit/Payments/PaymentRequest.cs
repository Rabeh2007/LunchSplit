namespace LunchSplit.Payments;

/// <summary>Amount is in dollars; the infrastructure converts it to cents.</summary>
public sealed record PaymentRequest(decimal Amount, string Currency,
    string IdempotencyKey, string CardNonce);
