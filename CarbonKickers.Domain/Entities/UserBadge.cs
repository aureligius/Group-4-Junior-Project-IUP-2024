namespace CarbonKickers.Domain.Entities;

public class UserBadge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int BadgeId { get; set; }
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}