namespace LunchSplit.Payments;

/// <summary>Coordinates payments through the gateway abstraction.</summary>
public sealed class PaymentManager
{
    private readonly IPaymentProcessor processor;

    public PaymentManager(IPaymentProcessor processor)
    {
        this.processor = processor ?? throw new ArgumentNullException(nameof(processor));
    }

    public async Task<PaymentResult> PayAsync(PaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        // Do not submit zero, negative, or fractional-cent amounts to the gateway.
        if (request.Amount <= 0m || decimal.Round(request.Amount, 2) != request.Amount)
            return new PaymentResult(false, ErrorCode: "INVALID_AMOUNT");

        try
        {
            return await processor.ProcessPaymentAsync(request);
        }
        catch (HttpRequestException error)
        {
            throw new PaymentGatewayTimeoutException("The payment gateway is unavailable.", error);
        }
        catch (OperationCanceledException error)
        {
            throw new PaymentGatewayTimeoutException("The payment gateway timed out.", error);
        }
    }
}
