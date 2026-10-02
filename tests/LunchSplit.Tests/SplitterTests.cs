using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunchSplit;

namespace LunchSplit.Tests;

[TestClass]
public class SplitterTests
{
    [TestMethod]
    public void ComputeTip_NoTipMode_ReturnsZero()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        decimal actual = splitter.ComputeTip(100m, TipMode.None, 15m);

        // Assert
        Assert.AreEqual(0m, actual);
    }

    [TestMethod]
    public void ComputeTip_PercentTipMode_ReturnsCorrectPercent()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        decimal actual = splitter.ComputeTip(120m, TipMode.Percent, 15m);

        // Assert
        Assert.AreEqual(18m, actual);
    }

    [TestMethod]
    public void ComputeTip_FixedTipMode_ReturnsFixedAmount()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        decimal actual = splitter.ComputeTip(100m, TipMode.Fixed, 15m);

        // Assert
        Assert.AreEqual(15m, actual);
    }

    [TestMethod]
    public void ComputeTip_NegativeSubtotal_ThrowsException()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        Action calculate = () => splitter.ComputeTip(-1m, TipMode.None, 0m);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(calculate);
    }

    [TestMethod]
    public void ComputeTip_NegativeFixedTip_ThrowsException()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        Action calculate = () => splitter.ComputeTip(100m, TipMode.Fixed, -1m);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(calculate);
    }

    [TestMethod]
    public void CalculateShares_EqualSplit_ApportionsEvenly()
    {
        // Arrange
        var bill = new Bill(90m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A"), new("B"), new("C") };

        // Act
        var actual = new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        CollectionAssert.AreEqual(new[] { 30m, 30m, 30m }, actual.Select(s => s.Amount).ToArray());
        Assert.AreEqual(90m, actual.Sum(s => s.Amount));
    }

    [TestMethod]
    public void CalculateShares_ProportionalSplit_ApportionsWeighted()
    {
        // Arrange
        var bill = new Bill(90m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A", 2), new("B", 1) };

        // Act
        var actual = new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(60m, actual[0].Amount);
        Assert.AreEqual(30m, actual[1].Amount);
        Assert.AreEqual(90m, actual.Sum(s => s.Amount));
    }

    [TestMethod]
    public void CalculateShares_ExcludedAttendee_ApportionsZero()
    {
        // Arrange
        var bill = new Bill(90m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A"), new("B"), new("C", 10, false) };

        // Act
        var actual = new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(45m, actual[0].Amount);
        Assert.AreEqual(45m, actual[1].Amount);
        Assert.AreEqual(0m, actual[2].Amount);
    }

    [TestMethod]
    public void CalculateShares_TaxAndPercentTip_ApportionsTotal()
    {
        // Arrange
        var bill = new Bill(100m, 13m, TipMode.Percent, 15m);
        var attendees = new List<Attendee> { new("A"), new("B") };

        // Act
        var actual = new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(64m, actual[0].Amount);
        Assert.AreEqual(64m, actual[1].Amount);
        Assert.AreEqual(128m, actual.Sum(s => s.Amount));
    }

    [TestMethod]
    public void CalculateShares_TaxAndFixedTip_ApportionsTotal()
    {
        // Arrange
        var bill = new Bill(100m, 13m, TipMode.Fixed, 20m);
        var attendees = new List<Attendee> { new("A"), new("B") };

        // Act
        var actual = new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(66.50m, actual[0].Amount);
        Assert.AreEqual(66.50m, actual[1].Amount);
        Assert.AreEqual(133m, actual.Sum(s => s.Amount));
    }

    [TestMethod]
    public void CalculateShares_AllAttendeesExcluded_ThrowsException()
    {
        // Arrange
        var bill = new Bill(10m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A", 1, false) };

        // Act
        Action calculate = () => new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.ThrowsException<ArgumentException>(calculate);
    }

    [TestMethod]
    public void CalculateShares_TotalWeightsZero_ThrowsException()
    {
        // Arrange
        var bill = new Bill(10m, 0m, TipMode.None, 0m);
        var attendees = new List<Attendee> { new("A", 0), new("B", 0) };

        // Act
        Action calculate = () => new Splitter().CalculateShares(bill, attendees, RoundingMode.Bankers);

        // Assert
        Assert.ThrowsException<ArgumentException>(calculate);
    }

    [TestMethod]
    public void ComputeTip_NegativePercentTip_ThrowsException()
    {
        // Arrange
        var splitter = new Splitter();

        // Act
        Action calculate = () => splitter.ComputeTip(100m, TipMode.Percent, -1m);

        // Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(calculate);
    }

}
