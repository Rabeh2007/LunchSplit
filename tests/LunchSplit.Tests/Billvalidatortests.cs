using Microsoft.VisualStudio.TestTools.UnitTesting;
using LunchSplit;

namespace LunchSplit.Tests
{
    [TestClass]
    public class BillValidatorTests
    {
        [TestMethod]
        public void Validate_CompleteBillDetails_ReturnsOk()
        {
            // Arrange
            var bill = new Bill(100m, 13m, TipMode.Percent, 15m);
            var attendees = new List<Attendee>();
            attendees.Add(new Attendee("A"));
            var validator = new BillValidator();

            // Act
            var actual = validator.Validate(bill, attendees);

            // Assert
            Assert.IsTrue(actual.IsValid);
            Assert.AreEqual("", actual.ErrorMessage);
        }

        [TestMethod]
        public void Validate_EmptyAttendeeCollection_ReturnsFail()
        {
            // Arrange
            var bill = new Bill(100m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>();

            // Act
            var actual = new BillValidator().Validate(bill, attendees);

            // Assert
            Assert.IsFalse(actual.IsValid);
            StringAssert.Contains(actual.ErrorMessage, "attendee");
        }

        [TestMethod]
        public void Validate_NegativeSubtotal_ReturnsFail()
        {
            // Arrange
            var bill = new Bill(-1m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee> { new Attendee("A") };

            // Act
            var actual = new BillValidator().Validate(bill, attendees);

            // Assert
            Assert.IsFalse(actual.IsValid);
            StringAssert.Contains(actual.ErrorMessage, "Subtotal");
        }

        [TestMethod]
        public void Validate_NegativeTax_ReturnsFail()
        {
            // Arrange
            var bill = new Bill(100m, -1m, TipMode.None, 0m);
            var attendees = new List<Attendee> { new Attendee("A") };

            // Act
            var actual = new BillValidator().Validate(bill, attendees);

            // Assert
            Assert.IsFalse(actual.IsValid);
            StringAssert.Contains(actual.ErrorMessage, "Tax");
        }

        [TestMethod]
        public void Validate_ZeroAttendees_ReturnsFail()
        {
            // Arrange
            var bill = new Bill(0m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>(capacity: 0);

            // Act
            var actual = new BillValidator().Validate(bill, attendees);

            // Assert
            Assert.IsFalse(actual.IsValid);
            Assert.IsFalse(string.IsNullOrWhiteSpace(actual.ErrorMessage));
        }

        [TestMethod]
        public void Validate_NegativeWeight_ReturnsFail()
        {
            // Arrange
            var bill = new Bill(10m, 0m, TipMode.None, 0m);
            var attendees = new List<Attendee>
            {
                new Attendee("A", -1)
            };

            // Act
            var actual = new BillValidator().Validate(bill, attendees);

            // Assert
            Assert.IsFalse(actual.IsValid);
            StringAssert.Contains(actual.ErrorMessage, "weights");
        }
    }
}