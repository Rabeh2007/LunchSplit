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
