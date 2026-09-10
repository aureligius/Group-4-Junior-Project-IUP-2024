namespace CarbonKickers.Domain.Entities;

public class LeagueParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int LeagueId { get; set; }
    public int Rank { get; set; }
    public int Xp { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    public void UpdateRank(int newRank) => Rank = newRank;
}