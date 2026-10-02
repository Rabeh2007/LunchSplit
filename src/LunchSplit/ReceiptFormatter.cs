using System.Globalization;
using System.Text;

namespace LunchSplit;

public class ReceiptFormatter
{
    private readonly string studentName;

    public ReceiptFormatter(string studentName = "Rabeh")
    {
        if (string.IsNullOrWhiteSpace(studentName))
            throw new ArgumentException("Student name is required.");
        this.studentName = studentName;
    }

    public string Format(Bill bill, List<Attendee> attendees, List<Share> roundedShares)
    {
        if (bill == null || attendees == null || roundedShares == null)
            throw new ArgumentNullException("Receipt inputs cannot be null.");
        if (attendees.Count != roundedShares.Count)
            throw new ArgumentException("Each attendee needs a matching share.");

        DateTime created = DateTime.Now;
        decimal tip = new Splitter().ComputeTip(bill.Subtotal, bill.TipMode, bill.TipInput);
        decimal grandTotal = 0;
        foreach (Share share in roundedShares)
            grandTotal += share.Amount;

        var receipt = new StringBuilder();
        receipt.AppendLine("LunchSplit Receipt");
        receipt.AppendLine("Student Name: " + studentName);
        receipt.AppendLine("Date: " + created.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        receipt.AppendLine("Created: " + created.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        receipt.AppendLine("Subtotal: " + Money(bill.Subtotal));
        receipt.AppendLine("Tax: " + Money(bill.Tax));
        receipt.AppendLine("Tip (" + bill.TipMode + "): " + Money(tip));
        receipt.AppendLine("Grand Total: " + Money(grandTotal));
        receipt.AppendLine("Attendee shares:");

        for (int i = 0; i < attendees.Count; i++)
        {
            if (attendees[i].Name != roundedShares[i].Name)
                throw new ArgumentException("Attendees and shares must use the same order.");
            string line = roundedShares[i].Name + ": " + Money(roundedShares[i].Amount);
            if (!attendees[i].Included)
                line += " (excluded)";
            receipt.AppendLine(line);
        }

        return receipt.ToString();
    }

    private string Money(decimal amount)
    {
        return "$" + amount.ToString("F2", CultureInfo.InvariantCulture);
    }
}
