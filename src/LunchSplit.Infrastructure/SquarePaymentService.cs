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

        try
        {
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
        catch (SquareApiException error)
        {
            // Square can return GENERIC_DECLINE for the declined magic nonce.
            // Expose the stable domain code while preserving the actual vendor code.
            var gatewayError = error.Errors.FirstOrDefault();
            var gatewayCode = gatewayError?.Code.ToString() ?? "GATEWAY_ERROR";
            var domainCode = gatewayCode == "GENERIC_DECLINE" ? "CARD_DECLINED" : gatewayCode;
            return new PaymentResult(false, ErrorCode: domainCode,
                HttpStatusCode: error.StatusCode, GatewayErrorCode: gatewayCode,
                GatewayErrorCategory: gatewayError?.Category.ToString(),
                GatewayErrorField: gatewayError?.Field);
        }
    }

    /// <summary>Retrieves the gateway state instead of assuming that a prior request succeeded.</summary>
    public async Task<PaymentStatus> GetTransactionStatusAsync(string transactionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        var response = await client.Payments.GetAsync(new GetPaymentsRequest
        {
            PaymentId = transactionId
        });
        // Preserve forward compatibility if Square introduces an unfamiliar status.
        return Enum.TryParse<PaymentStatus>(response.Payment?.Status, out var status)
            && Enum.IsDefined(status) ? status : PaymentStatus.UNKNOWN;
    }

    public void Dispose() => httpClient.Dispose();
}
