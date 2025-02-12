namespace ZoneRV.Models.Production;

/// <summary>
/// Represents statistical information about a sales order, including job and card completion statuses.
/// </summary>
[DebuggerDisplay("JobCompleted:{JobCompleted} JobCardsDue:{JobCardsDue} JobCardsOutStanding:{JobCardsOutStanding} RedCardsCompleted:{RedCardsCompleted} RedCardsIncomplete:{RedCardsIncomplete} YellowCardsCompleted:{YellowCardsCompleted} YellowCardsIncomplete:{YellowCardsIncomplete}")]
public class SalesOrderStats
{
    public SalesOrderStats(SalesOrder so, Func<Card, bool>? cardFilter = null)
    {
        List<JobCard>    jobs   = so.JobCards.Where(cardFilter ?? (_ => true)).OfType<JobCard>().ToList();
        List<RedCard>    reds   = so.RedCards.Where(cardFilter ?? (_ => true)).OfType<RedCard>().ToList();
        List<YellowCard> yellow = so.YellowCards.Where(cardFilter ?? (_ => true)).OfType<YellowCard>().ToList();

        JobCompleted = jobs.Count(x => x.CardStatus is CardStatus.Completed);
        JobCardsDue = jobs.Count(x => x.CardStatus is not CardStatus.Completed && x.DueStatus is not DueStatus.Due);
        JobCardsOutStanding = jobs.Count(x => x.CardStatus is not CardStatus.Completed && x.DueStatus is not DueStatus.OverDue);

        RedCardsCompleted   = reds.Count(x => x.CardStatus is CardStatus.Completed);
        RedCardsIncomplete = reds.Count(x => x.CardStatus is not CardStatus.Completed);

        YellowCardsCompleted   = yellow.Count(x => x.CardStatus is CardStatus.Completed);
        YellowCardsIncomplete = yellow.Count(x => x.CardStatus is not CardStatus.Completed);
    }
    
    public int JobCompleted;
    public int JobCardsDue;
    public int JobCardsOutStanding;
    public int RedCardsCompleted;
    public int RedCardsIncomplete;
    public int YellowCardsCompleted;
    public int YellowCardsIncomplete;
}