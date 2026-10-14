using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunchSplit;

namespace LunchSplit.Tests;

/// <summary>
/// Tests the returning CalculateShares paths in the A2 report.
/// </summary>
[TestClass]
public class CalculateSharesBasisPathTests
{
    /// <summary>
    /// Provides inputs and expected amounts for P1 and P4-P7.
    /// </summary>
    public static IEnumerable<object[]> ReturningPaths => new List<object[]>
    {
        // P1: Bankers rounding changes 10.005 to 10.00
        new object[] { "P1", new Bill(10m, 0m, TipMode.Fixed, 0.005m),
            new List<Attendee> { new("A") },
            RoundingMode.Bankers, new[] { 10.00m } },

        // P4: No rounding so the amount stays 10.005.
        new object[] { "P4", new Bill(10m, 0m, TipMode.Fixed, 0.005m),
            new List<Attendee> { new("A") },
            RoundingMode.None, new[] { 10.005m } },

        // P5: loops here run 2 times.(128 split 64/64)
        new object[] { "P5", new Bill(100m, 13m, TipMode.Percent, 15m),
            new List<Attendee> { new("A"), new("B") },
            RoundingMode.Bankers, new[] { 64m, 64m } },

        // P6: The excluded attendee pays zero
        new object[] { "P6", new Bill(90m, 0m, TipMode.None, 0m),
            new List<Attendee> { new("A"), new("B", 1, false) },
            RoundingMode.Bankers, new[] { 90m, 0m } },

        // P7: The zero weight attendee doesnt pay 
        new object[] { "P7", new Bill(90m, 0m, TipMode.None, 0m),
            new List<Attendee> { new("A"), new("B", 0) },
            RoundingMode.Bankers, new[] { 90m, 0m } },
            };



    ///                   <summary>
   ///This is for expected amounts for each returning path.
    ///                   </summary>
    [DataTestMethod]
    [DynamicData(nameof(ReturningPaths))]
    public void CalculateShares_BasisPath_ReturnsExpectedShares(
        string pathId, Bill bill, List<Attendee> attendees,
        RoundingMode roundingMode, decimal[] expected)
    {
       
        var splitter = new Splitter();

        List<Share> actual = splitter.CalculateShares(
            bill, attendees, roundingMode);

        
        CollectionAssert.AreEqual(
            expected,
            actual.Select(s => s.Amount).ToArray(),
            "Wrong share amounts for " + pathId);
    }
}

