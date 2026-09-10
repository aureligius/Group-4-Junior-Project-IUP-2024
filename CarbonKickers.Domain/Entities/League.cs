namespace CarbonKickers.Domain.Entities;

public class League
{
    public int Id { get; set; }
    public string TierName { get; set; } = string.Empty;
    public int MinXp { get; set; }
    public int MaxXp { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsEligible(int xp) => xp >= MinXp && xp <= MaxXp;
}