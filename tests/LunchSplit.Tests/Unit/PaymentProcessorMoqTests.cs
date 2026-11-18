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
    public async Task M3_07_SecondAttendeeDeclines_ReportsTwoSuccessesAndOneFailure()
    {
        // Arrange: distinct requests let the mock identify each attendee precisely.
        var requests = new[] { Request(10m), Request(20m), Request(30m) };
        var processor = new Mock<IPaymentProcessor>(MockBehavior.Strict);
        processor.Setup(p => p.ProcessPaymentAsync(requests[0]))
            .ReturnsAsync(new PaymentResult(true, "attendee-1"));
        processor.Setup(p => p.ProcessPaymentAsync(requests[1]))
            .ReturnsAsync(new PaymentResult(false, ErrorCode: "CARD_DECLINED"));
        processor.Setup(p => p.ProcessPaymentAsync(requests[2]))
            .ReturnsAsync(new PaymentResult(true, "attendee-3"));
        var manager = new PaymentManager(processor.Object);

        // Act: a declined card must not prevent attendee 3 from being processed.
        var summary = await manager.PayBatchAsync(requests);

        // Assert: retain the individual decline alongside the successful payments.
        Assert.AreEqual(2, summary.SuccessCount);
        Assert.AreEqual(1, summary.FailureCount);
        Assert.AreEqual("CARD_DECLINED", summary.Results[1].ErrorCode);
        Assert.AreEqual("attendee-3", summary.Results[2].TransactionId);
        foreach (var request in requests)
            processor.Verify(p => p.ProcessPaymentAsync(request), Times.Once);
        processor.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task M3_06_FourAttendeeBill_InvokesGatewayExactlyFourTimes()
    {
        // Arrange: use Assignment 1's real splitter, with four equally weighted attendees.
        var bill = new Bill(40m, 0m, TipMode.None, 0m);
        var attendees = Enumerable.Range(1, 4)
            .Select(i => new Attendee($"Attendee {i}", 1, true)).ToList();
        var requests = new List<PaymentRequest>();
        var processor = new Mock<IPaymentProcessor>(MockBehavior.Strict);
        processor.Setup(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()))
            .Callback<PaymentRequest>(requests.Add)
            .ReturnsAsync(new PaymentResult(true, "transaction-06"));
        var manager = new PaymentManager(processor.Object);

        // Act.
        var summary = await manager.PayBillAsync(bill, attendees, "CAD",
            "cnon:card-nonce-ok", "batch-06");

        // Assert: four calls, exact total, and a distinct retry key for each share.
        processor.Verify(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()), Times.Exactly(4));
        Assert.AreEqual(4, summary.SuccessCount);
        Assert.AreEqual(40m, requests.Sum(r => r.Amount));
        Assert.IsTrue(requests.All(r => r.Amount == 10m));
        Assert.AreEqual(4, requests.Select(r => r.IdempotencyKey).Distinct().Count());
        processor.VerifyNoOtherCalls();
    }

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
    public async Task M3_05_ZeroDollarRequest_NeverInvokesGateway()
    {
        // Arrange: a strict mock also fails if an unexpected call slips through.
        var processor = new Mock<IPaymentProcessor>(MockBehavior.Strict);
        var manager = new PaymentManager(processor.Object);

        // Act: a free bill should be rejected before contacting Square.
        var result = await manager.PayAsync(Request(0m));

        // Assert.
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("INVALID_AMOUNT", result.ErrorCode);
        processor.Verify(p => p.ProcessPaymentAsync(It.IsAny<PaymentRequest>()), Times.Never);
        processor.VerifyNoOtherCalls();
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
