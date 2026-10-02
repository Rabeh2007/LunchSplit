using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LunchSplit.Tests;

[TestClass]
public class FutureFeatureTests
{
    [TestMethod]
    [TestCategory("FutureFeature")]
    [Ignore("TDD-26 / F5: CSV exporting is pending for a future assignment.")]
    public void Format_CsvReceiptExporter_MatchesSchema()
    {
        // Arrange
        string expectedSchema = "Name,Amount";

        // Act
        // Add the CSV export test in a later!
        string? actualSchema = null;

        // Assert
        Assert.AreEqual(expectedSchema, actualSchema,
            "RED/PENDING: CSV exporter is not implemented in Assignment 1.");
    }

    [TestMethod]
    [TestCategory("FutureFeature")]
    [Ignore("TDD-27 / F6: Stripe mock payment requests are pending.")]
    public void CalculateShares_PaymentRequest_ValidatesStripeMock()
    {
        // Arrange
        bool expectedRequestAccepted = true;

        // Act
        // // Add the Stripe mock test when we work on payments in a later assignment.
        bool actualRequestAccepted = false;

        // Assert
        Assert.AreEqual(expectedRequestAccepted, actualRequestAccepted,
            "RED/PENDING: Stripe gateway is not implemented in Assignment 1.");
    }
}
