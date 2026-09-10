namespace CarbonKickers.Domain.Entities;

using CarbonKickers.Domain.Enums;

public class Challenge
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChallengerId { get; set; }
    public Guid OpponentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double ChallengerSavedKg { get; set; }
    public double OpponentSavedKg { get; set; }
    public ChallengeStatus Status { get; set; } = ChallengeStatus.Pending;
    public Guid? WinnerId { get; private set; }

    public void AcceptChallenge()
    {
        Status = ChallengeStatus.Accepted;
    }

    public Guid ResolveWinner()
    {
        Status = ChallengeStatus.Completed;
        WinnerId = ChallengerSavedKg >= OpponentSavedKg ? ChallengerId : OpponentId;
        return WinnerId.Value;
    }
}