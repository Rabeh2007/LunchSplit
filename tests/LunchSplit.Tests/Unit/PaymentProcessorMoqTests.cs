using LunchSplit.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace LunchSplit.Tests.Unit;

[TestClass]
[TestCategory("Moq")]
public class PaymentProcessorMoqTests
{
    private static PaymentRequest Request(decimal amount = 12.50m) =>
        new(amount, "CAD", Guid.NewGuid().ToString(), "cnon:card-nonce-ok");

    [TestMethod]
    public async Task M3_03_NetworkTimeout_ThrowsDomainTimeoutException()
    {
        // Arrange: no network is used; the mock simulates a failed HTTP request.
        var transportError = new HttpRequestException("Gateway timed out.");
        var processor = new Mock<IPaymentProcessor>(MockBehavior.Strict);
        processor.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
            .ThrowsAsync(transportError);
        var manager = new PaymentManager(processor.Object);

        // Act and assert: preserve the transport error for diagnostics.
        var error = await Assert.ThrowsExceptionAsync<PaymentGatewayTimeoutException>(
            () => manager.PayAsync(Request()));
        Assert.AreSame(transportError, error.InnerException);
    }
}
