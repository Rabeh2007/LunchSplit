namespace LunchSplit;

public class BillValidator
{
    public ValidationResult Validate(Bill bill, List<Attendee> attendees)
    {
        if (bill == null)
            return new ValidationResult(false, "Please provide a bill.");
        if (attendees == null || attendees.Count == 0)
            return new ValidationResult(false, "Please add at least one attendee.");
        if (bill.Subtotal < 0)
            return new ValidationResult(false, "Subtotal cannot be negative.");
        if (bill.Tax < 0)
            return new ValidationResult(false, "Tax cannot be negative.");
        if (!Enum.IsDefined(bill.TipMode))
            return new ValidationResult(false, "Unknown tip mode.");
        if (bill.TipMode != TipMode.None && bill.TipInput < 0)
            return new ValidationResult(false, "Tip input cannot be negative.");

        int includedCount = 0;
        decimal totalWeight = 0;
        foreach (Attendee attendee in attendees)
        {
            if (attendee == null || string.IsNullOrWhiteSpace(attendee.Name))
                return new ValidationResult(false, "Each attendee needs a name.");
            if (attendee.Weight < 0)
                return new ValidationResult(false, "Attendee weights cannot be negative.");
            if (attendee.Included)
            {
                includedCount++;
                totalWeight += attendee.Weight;
            }
        }

        if (includedCount == 0)
            return new ValidationResult(false, "Include at least one attendee.");
        if (totalWeight == 0)
            return new ValidationResult(false, "Included weights must total more than zero.");

        return new ValidationResult(true, "");
    }
}
