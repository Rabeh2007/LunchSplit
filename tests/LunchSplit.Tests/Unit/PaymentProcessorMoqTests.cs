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

    [TestMethod]
    public async Task M3_04_Payment_ForwardsExactDollarAmountOnce()
    {
        // Arrange: a fractional dollar amount exposes accidental cents conversion here.
        var request = Request(23.47m);
        var expected = new PaymentResult(true, "transaction-04");
        var processor = new Mock<IPaymentProcessor>(MockBehavior.Strict);
        processor.Setup(p => p.ProcessPaymentAsync(request)).ReturnsAsync(expected);
        var manager = new PaymentManager(processor.Object);

        // Act.
        var result = await manager.PayAsync(request);

        // Assert: preserve every request field and invoke the boundary only once.
        Assert.AreSame(expected, result);
        processor.Verify(p => p.ProcessPaymentAsync(It.Is<PaymentRequest>(r =>
            r.Amount == 23.47m && r.Currency == request.Currency &&
            r.IdempotencyKey == request.IdempotencyKey && r.CardNonce == request.CardNonce)),
            Times.Once);
        processor.VerifyNoOtherCalls();
    }
}
