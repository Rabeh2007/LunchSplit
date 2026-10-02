namespace LunchSplit;

public class Rounder
{
    public List<Share> RoundShares(List<Share> rawShares, RoundingMode mode)
    {
        if (rawShares == null)
            throw new ArgumentNullException(nameof(rawShares));
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));

        var roundedShares = new List<Share>();
        decimal rawTotal = 0;
        decimal roundedTotal = 0;
        foreach (Share share in rawShares)
        {
            if (share == null || share.Amount < 0)
                throw new ArgumentException("Shares must be non-negative and non-null.");

            decimal amount = RoundAmount(share.Amount, mode);
            roundedShares.Add(new Share(share.Name, amount));
            rawTotal += share.Amount;
            roundedTotal += amount;
        }

        if (mode == RoundingMode.None || rawShares.Count == 0)
            return roundedShares;

        decimal target = RoundAmount(rawTotal, mode);
        decimal remainder = target - roundedTotal;

        // Adjust one penny at a time until the shares match the total.
        while (remainder != 0)
        {
            bool addPenny = remainder > 0;
            int bestIndex = -1;
            decimal bestDifference = decimal.MinValue;

            for (int i = 0; i < rawShares.Count; i++)
            {
                // Excluded attendees and zero-weight shares must stay at zero.
                if (rawShares[i].Amount == 0)
                    continue;
                if (!addPenny && roundedShares[i].Amount < 0.01m)
                    continue;

                decimal difference;
                if (addPenny)
                    difference = rawShares[i].Amount - roundedShares[i].Amount;
                else
                    difference = roundedShares[i].Amount - rawShares[i].Amount;

                // Treat tiny decimal-division differences as ties.
                difference = decimal.Round(difference, 12);
                if (difference > bestDifference)
                {
                    bestDifference = difference;
                    bestIndex = i;
                }
            }

            if (bestIndex == -1)
                throw new InvalidOperationException("Could not reconcile the shares.");

            decimal penny = addPenny ? 0.01m : -0.01m;
            Share selected = roundedShares[bestIndex];
            roundedShares[bestIndex] = new Share(selected.Name, selected.Amount + penny);
            remainder -= penny;
        }

        return roundedShares;
    }

    private decimal RoundAmount(decimal amount, RoundingMode mode)
    {
        switch (mode)
        {
            case RoundingMode.None:
                return amount;
            case RoundingMode.Bankers:
                return decimal.Round(amount, 2, MidpointRounding.ToEven);
            case RoundingMode.Up:
                return decimal.Ceiling(amount * 100m) / 100m;
            case RoundingMode.Down:
                return decimal.Floor(amount * 100m) / 100m;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
