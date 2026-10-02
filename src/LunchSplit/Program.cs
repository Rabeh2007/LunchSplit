using LunchSplit;

var bill = new Bill(100m, 13m, TipMode.Percent, 15m);
var attendees = new List<Attendee>
{
    new Attendee("Rabeh"),
    new Attendee("Alex"),
    new Attendee("Sam"),
    new Attendee("Taylor", 1, false)
};

var splitter = new Splitter();
var shares = splitter.CalculateShares(bill, attendees, RoundingMode.Bankers);
var formatter = new ReceiptFormatter("Rabeh");
Console.WriteLine(formatter.Format(bill, attendees, shares));

Console.WriteLine("Penny reconciliation example ($10 / 3):");
var pennyBill = new Bill(10m, 0m, TipMode.None, 0m);
var pennyAttendees = new List<Attendee>
{
    new Attendee("Rabeh"),
    new Attendee("Alex"),
    new Attendee("Sam")
};
var pennyShares = splitter.CalculateShares(pennyBill, pennyAttendees, RoundingMode.Bankers);
Console.WriteLine(formatter.Format(pennyBill, pennyAttendees, pennyShares));
