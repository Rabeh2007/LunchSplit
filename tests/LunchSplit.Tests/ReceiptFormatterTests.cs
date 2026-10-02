using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunchSplit;

namespace LunchSplit.Tests;

[TestClass]
public class ReceiptFormatterTests
{
    [TestMethod]
    public void Format_DefaultBillReceipt_ContainsLineItems()
    {
        // Arrange
        var bill = new Bill(100m, 13m, TipMode.Percent, 15m);
        var attendees = new List<Attendee> { new("A"), new("B") };
        var shares = new List<Share> { new("A", 64m), new("B", 64m) };

        // Act
        string actual = new ReceiptFormatter().Format(bill, attendees, shares);

        // Assert
        StringAssert.Contains(actual, "Subtotal: $100.00");
        StringAssert.Contains(actual, "Tax: $13.00");
        StringAssert.Contains(actual, "Tip (Percent): $15.00");
        StringAssert.Contains(actual, "Grand Total: $128.00");
        CollectionAssert.Contains(actual.Split(Environment.NewLine), "A: $64.00");
        CollectionAssert.Contains(actual.Split(Environment.NewLine), "B: $64.00");
    }

    [TestMethod]
    public void Format_ReceiptOutput_ContainsRequiredMetadata()
    {
        // Arrange
        var bill = new Bill(10m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A") };
        var shares = new List<Share> { new("A", 10m) };
        var formatter = new ReceiptFormatter("Rabeh");
        DateTime before = DateTime.Now.AddSeconds(-1);

        // Act
        string actual = formatter.Format(bill, attendees, shares);

        // Assert
        StringAssert.Contains(actual, "Student Name: Rabeh");
        string createdLine = actual.Split(Environment.NewLine)
            .First(line => line.StartsWith("Created: "));
        DateTime created = DateTime.ParseExact(createdLine.Substring(9),
            "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        Assert.IsTrue(created >= before && created <= DateTime.Now);
        StringAssert.Contains(actual, "Date: " + created.ToString("yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture));
    }

}
