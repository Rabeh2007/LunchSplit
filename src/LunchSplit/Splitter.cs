namespace LunchSplit;

public class Splitter
{
    public decimal ComputeTip(decimal subtotal, TipMode mode, decimal tipInput)
    {
        if (subtotal < 0)
            throw new ArgumentOutOfRangeException(nameof(subtotal));
        if (mode != TipMode.None && tipInput < 0)
            throw new ArgumentOutOfRangeException(nameof(tipInput));

        switch (mode)
        {
            case TipMode.None:
                return 0m;
            case TipMode.Percent:
                return subtotal * tipInput / 100m;
            case TipMode.Fixed:
                return tipInput;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }

    public List<Share> CalculateShares(Bill bill, List<Attendee> attendees,
        RoundingMode roundingMode)
    {
        var validator = new BillValidator();
        ValidationResult result = validator.Validate(bill, attendees);
        if (!result.IsValid)
            throw new ArgumentException(result.ErrorMessage);
        if (!Enum.IsDefined(roundingMode))
            throw new ArgumentOutOfRangeException(nameof(roundingMode));

        decimal total = bill.Subtotal + bill.Tax
            + ComputeTip(bill.Subtotal, bill.TipMode, bill.TipInput);
        if (roundingMode != RoundingMode.None)
            total = decimal.Round(total, 2, MidpointRounding.ToEven);

        decimal totalWeight = 0;
        foreach (Attendee attendee in attendees)
        {
            if (attendee.Included)
                totalWeight += attendee.Weight;
        }

        var rawShares = new List<Share>();
        decimal allocated = 0;
        int lastPayingIndex = -1;
        for (int i = 0; i < attendees.Count; i++)
        {
            Attendee attendee = attendees[i];
            decimal amount = 0;
            if (attendee.Included && attendee.Weight > 0)
            {
                amount = total * attendee.Weight / totalWeight;
                lastPayingIndex = i;
            }
            rawShares.Add(new Share(attendee.Name, amount));
            allocated += amount;
        }

        // Decimal division can leave a tiny remainder. Keep the raw total exact.
        decimal residual = total - allocated;
        Share lastShare = rawShares[lastPayingIndex];
        rawShares[lastPayingIndex] = new Share(lastShare.Name,
            lastShare.Amount + residual);

        var rounder = new Rounder();
        return rounder.RoundShares(rawShares, roundingMode);
    }
}
