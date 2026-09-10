namespace CarbonKickers.Domain.Entities;

public class Badge
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int XpRequirement { get; set; }
    public string IconUrl { get; set; } = string.Empty;

    public bool IsEligible(int userXp) => userXp >= XpRequirement;
}