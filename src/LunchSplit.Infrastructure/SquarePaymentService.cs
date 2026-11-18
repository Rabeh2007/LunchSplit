using LunchSplit.Payments;
using Square;
using Square.Payments;

namespace LunchSplit.Infrastructure;

/// <summary>Translates domain payments to the official Square SDK in Sandbox only.</summary>
public sealed class SquarePaymentService : IPaymentProcessor, IDisposable
{
    private readonly SquareClient client;
    private readonly HttpClient httpClient;
    private readonly string locationId;

    public SquarePaymentService(string accessToken, string locationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(locationId);
        this.locationId = locationId;
        httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        client = new SquareClient(accessToken, new ClientOptions
        {
            BaseUrl = SquareEnvironment.Sandbox,
            HttpClient = httpClient,
            Timeout = TimeSpan.FromSeconds(5),
            MaxRetries = 0
        });
    }

    public static SquarePaymentService FromEnvironment()
    {
        string Required(string name) => Environment.GetEnvironmentVariable(name)
            is { Length: > 0 } value ? value
            : throw new InvalidOperationException($"Set {name} before running Sandbox tests.");

        return new SquarePaymentService(Required("SQUARE_SANDBOX_ACCESS_TOKEN"),
            Required("SQUARE_SANDBOX_LOCATION_ID"));
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Amount <= 0m || decimal.Round(request.Amount, 2) != request.Amount)
            return new PaymentResult(false, ErrorCode: "INVALID_AMOUNT");
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Currency);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.IdempotencyKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.CardNonce);

        // Convert dollars exactly once, at the vendor boundary. No floating point arithmetic.
        var response = await client.Payments.CreateAsync(new CreatePaymentRequest
        {
            SourceId = request.CardNonce,
            IdempotencyKey = request.IdempotencyKey,
            AmountMoney = new Money
            {
                Amount = checked((long)(request.Amount * 100m)),
                Currency = new Currency(request.Currency.ToUpperInvariant())
            },
            LocationId = locationId,
            Autocomplete = true
        });
        var payment = response.Payment;
        return payment?.Status is "COMPLETED" or "APPROVED"
            ? new PaymentResult(true, payment.Id, HttpStatusCode: 200)
            : new PaymentResult(false, payment?.Id, "PAYMENT_NOT_COMPLETED", 200);
    }

    public Task<PaymentStatus> GetTransactionStatusAsync(string transactionId)
    {
        throw new NotImplementedException();
    }

    public void Dispose() => httpClient.Dispose();
}
