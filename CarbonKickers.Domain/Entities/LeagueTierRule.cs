namespace CarbonKickers.Domain.Entities;

public class LeagueTierRule
{
    public int Id { get; set; }

    public string TierName { get; set; } = string.Empty;

    public int MinXp { get; set; }

    // Null means there is no upper limit.
    public int? MaxXp { get; set; }

    public bool IsActive { get; set; } = true;
}