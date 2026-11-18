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
