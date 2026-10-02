using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunchSplit;

namespace LunchSplit.Tests;



[TestClass]
public class RounderTests
{
    [TestMethod]
    public void RoundShares_NoRounding_PreservesRawDecimals()
    {
        // Arrange
        var raw = new List<Share> { new("A", 10.123456m), new("B", 2.987654m) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.None);

        // Assert
        CollectionAssert.AreEqual(raw, actual);
        Assert.AreNotSame(raw, actual);
    }

    [TestMethod]
    public void RoundShares_BankersRounding_RoundsToNearestEven()
    {
        // Arrange
        var raw = new List<Share> { new("A", 10.325m), new("B", 10.335m) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(10.32m, actual[0].Amount);
        Assert.AreEqual(10.34m, actual[1].Amount);
    }

    [TestMethod]
    public void RoundShares_RoundUp_AppliesCeilingToCents()
    {
        // Arrange
        var raw = new List<Share> { new("A", 10.331m) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Up);

        // Assert
        Assert.AreEqual(10.34m, actual[0].Amount);
    }

    [TestMethod]
    public void RoundShares_RoundDown_AppliesFloorToCents()
    {
        // Arrange
        var raw = new List<Share> { new("A", 10.339m) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Down);

        // Assert
        Assert.AreEqual(10.33m, actual[0].Amount);
    }

    [TestMethod]
    public void RoundShares_UnevenSplit_ReconcilesRemainder()
    {
        // Arrange
        decimal third = 10m / 3m;
        var raw = new List<Share> { new("A", third), new("B", third), new("C", third) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Bankers);

        // Assert
        CollectionAssert.AreEqual(new[] { 3.34m, 3.33m, 3.33m }, actual.Select(s => s.Amount).ToArray());
        Assert.AreEqual(10m, actual.Sum(s => s.Amount));
    }

    [TestMethod]
    public void RoundShares_EmptyShareCollection_ReturnsEmpty()
    {
        // Arrange
        var raw = new List<Share>();

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Bankers);

        // Assert
        Assert.AreEqual(0, actual.Count);
    }

    [TestMethod]
    public void RoundShares_UpUnevenSplit_PreservesTotalAndZeroShares()
    {
        // Arrange
        decimal third = 10m / 3m;
        var raw = new List<Share> { new("Excluded", 0m), new("A", third), new("B", third), new("C", third) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Up);

        // Assert
        Assert.AreEqual(10m, actual.Sum(s => s.Amount));
        Assert.AreEqual(0m, actual[0].Amount);
        Assert.IsTrue(actual.All(s => s.Amount >= 0m && s.Amount * 100m == decimal.Truncate(s.Amount * 100m)));
    }

    [TestMethod]
    public void RoundShares_DownUnevenSplit_PreservesTotalAndZeroShares()
    {
        // Arrange
        decimal third = 10m / 3m;
        var raw = new List<Share> { new("Excluded", 0m), new("A", third), new("B", third), new("C", third) };

        // Act
        var actual = new Rounder().RoundShares(raw, RoundingMode.Down);

        // Assert
        Assert.AreEqual(10m, actual.Sum(s => s.Amount));
        
        
        Assert.AreEqual(0m, actual[0].Amount);
        
        Assert.IsTrue(actual.All(s => s.Amount >= 0m && s.Amount * 100m == decimal.Truncate(s.Amount * 100m)));
    }

}
