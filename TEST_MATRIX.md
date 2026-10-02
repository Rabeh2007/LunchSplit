# Required Assignment 1 Test Matrix

Each entry below is a distinct test method. TDD-26 and TDD-27 are intentionally
pending/ignored, as permitted by the RED/PENDING requirement.

| ID | Method | File |
| --- | --- | --- |
| M1-1 | `ComputeTip_NoTipMode_ReturnsZero` | `SplitterTests.cs` |
| M1-2 | `ComputeTip_PercentTipMode_ReturnsCorrectPercent` | `SplitterTests.cs` |
| M1-3 | `ComputeTip_FixedTipMode_ReturnsFixedAmount` | `SplitterTests.cs` |
| M1-4 | `ComputeTip_NegativeSubtotal_ThrowsException` | `SplitterTests.cs` |
| M1-5 | `ComputeTip_NegativeFixedTip_ThrowsException` | `SplitterTests.cs` |
| M2-6 | `RoundShares_NoRounding_PreservesRawDecimals` | `RounderTests.cs` |
| M2-7 | `RoundShares_BankersRounding_RoundsToNearestEven` | `RounderTests.cs` |
| M2-8 | `RoundShares_RoundUp_AppliesCeilingToCents` | `RounderTests.cs` |
| M2-9 | `RoundShares_RoundDown_AppliesFloorToCents` | `RounderTests.cs` |
| M2-10 | `RoundShares_UnevenSplit_ReconcilesRemainder` | `RounderTests.cs` |
| M2-11 | `RoundShares_EmptyShareCollection_ReturnsEmpty` | `RounderTests.cs` |
| M3-12 | `Validate_CompleteBillDetails_ReturnsOk` | `BillValidatorTests.cs` |
| M3-13 | `Validate_EmptyAttendeeCollection_ReturnsFail` | `BillValidatorTests.cs` |
| M3-14 | `Validate_NegativeSubtotal_ReturnsFail` | `BillValidatorTests.cs` |
| M3-15 | `Validate_NegativeTax_ReturnsFail` | `BillValidatorTests.cs` |
| M3-16 | `Validate_ZeroAttendees_ReturnsFail` | `BillValidatorTests.cs` |
| M4-17 | `CalculateShares_EqualSplit_ApportionsEvenly` | `SplitterTests.cs` |
| M4-18 | `CalculateShares_ProportionalSplit_ApportionsWeighted` | `SplitterTests.cs` |
| M4-19 | `CalculateShares_ExcludedAttendee_ApportionsZero` | `SplitterTests.cs` |
| M4-20 | `CalculateShares_TaxAndPercentTip_ApportionsTotal` | `SplitterTests.cs` |
| M4-21 | `CalculateShares_TaxAndFixedTip_ApportionsTotal` | `SplitterTests.cs` |
| M4-22 | `CalculateShares_AllAttendeesExcluded_ThrowsException` | `SplitterTests.cs` |
| M4-23 | `CalculateShares_TotalWeightsZero_ThrowsException` | `SplitterTests.cs` |
| M5-24 | `Format_DefaultBillReceipt_ContainsLineItems` | `ReceiptFormatterTests.cs` |
| M5-25 | `Format_ReceiptOutput_ContainsRequiredMetadata` | `ReceiptFormatterTests.cs` |
| TDD-26 | `Format_CsvReceiptExporter_MatchesSchema` | `FutureFeatureTests.cs` |
| TDD-27 | `CalculateShares_PaymentRequest_ValidatesStripeMock` | `FutureFeatureTests.cs` |
