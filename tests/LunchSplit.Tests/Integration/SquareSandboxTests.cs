using LunchSplit.Infrastructure;
using LunchSplit.Payments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchSplit.Tests.Integration;

[TestClass]
[TestCategory("SquareSandbox")]
public class SquareSandboxTests
{
    private static PaymentRequest Request(string nonce = "cnon:card-nonce-ok",
        string currency = "CAD") => new(1.00m, currency, Guid.NewGuid().ToString(), nonce);

    [TestMethod]
    public async Task M3_10_CompletedPayment_StatusQueryReturnsCompleted()
    {
        // Arrange: create this test's own payment so it never relies on test execution order.
        using var service = SquarePaymentService.FromEnvironment();
        var payment = await service.ProcessPaymentAsync(Request());
        Assert.IsTrue(payment.IsSuccess, $"Payment setup failed: {payment.ErrorCode}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(payment.TransactionId));

        // Act: retrieve the stored payment through GET /v2/payments/{payment_id}.
        var status = await service.GetTransactionStatusAsync(payment.TransactionId!);

        // Assert: the domain state matches the completed Sandbox transaction.
        Assert.AreEqual(PaymentStatus.COMPLETED, status);
    }

    [TestMethod]
    public async Task M3_09_DuplicateIdempotencyKey_ReturnsOriginalTransaction()
    {
        // Arrange: a new key isolates this test run from previous Sandbox payments.
        using var service = SquarePaymentService.FromEnvironment();
        var request = Request();

        // Act: resend the identical payment, as a client would after losing a response.
        var original = await service.ProcessPaymentAsync(request);
        Assert.IsTrue(original.IsSuccess, $"Original payment failed: {original.ErrorCode}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(original.TransactionId));
        var retry = await service.ProcessPaymentAsync(request);

        // Assert: Square returns the original payment rather than creating a second charge.
        Assert.IsTrue(retry.IsSuccess, $"Retried payment failed: {retry.ErrorCode}");
        Assert.AreEqual(200, original.HttpStatusCode);
        Assert.AreEqual(200, retry.HttpStatusCode);
        Assert.AreEqual(original.TransactionId, retry.TransactionId);
    }

    [TestMethod]
    public async Task M3_08_EurPaymentAtCadLocation_ReturnsHttp400ValidationError()
    {
        // Arrange: EUR is valid currency syntax but is unsupported by our CAD location.
        // Send it to Square so this test exercises gateway validation, not a local guard.
        using var service = SquarePaymentService.FromEnvironment();

        // Act.
        var result = await service.ProcessPaymentAsync(Request(currency: "EUR"));

        // Assert: a currency mismatch must fail without a successful transaction.
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(400, result.HttpStatusCode);
        Assert.AreEqual("INVALID_REQUEST_ERROR", result.GatewayErrorCategory);
        Assert.AreEqual("amount_money.currency", result.GatewayErrorField);
        Assert.AreEqual("INVALID_VALUE", result.GatewayErrorCode);
        Assert.AreEqual(result.GatewayErrorCode, result.ErrorCode);
        Assert.IsTrue(string.IsNullOrEmpty(result.TransactionId));
    }

    [TestMethod]
    public async Task M3_02_DeclinedMagicNonce_ReturnsCardDeclined()
    {
        // Arrange: Square's magic nonce deterministically declines the test card.
        using var service = SquarePaymentService.FromEnvironment();

        // Act.
        var result = await service.ProcessPaymentAsync(Request("cnon:card-nonce-declined"));

        // Assert: normalize a generic decline without losing the original gateway code.
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("CARD_DECLINED", result.ErrorCode);
        Assert.AreEqual(400, result.HttpStatusCode);
        Assert.IsTrue(result.GatewayErrorCode is "CARD_DECLINED" or "GENERIC_DECLINE",
            $"Unexpected gateway decline code: {result.GatewayErrorCode}");
    }

    [TestMethod]
    public async Task M3_01_ValidMagicNonce_CreatesSandboxPayment()
    {
        // Arrange: this is a real Sandbox request using externally configured credentials.
        using var service = SquarePaymentService.FromEnvironment();

        // Act.
        var result = await service.ProcessPaymentAsync(Request());

        // Assert: never treat a nonempty failure response as an authorization.
        Assert.IsTrue(result.IsSuccess, $"Square rejected payment: {result.ErrorCode}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.TransactionId));
    }
}
