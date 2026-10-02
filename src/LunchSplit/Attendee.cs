namespace LunchSplit;

public sealed record Attendee(string Name, int Weight = 1, bool Included = true);
