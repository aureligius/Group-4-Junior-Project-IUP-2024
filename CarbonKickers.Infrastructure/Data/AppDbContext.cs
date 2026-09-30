using Microsoft.EntityFrameworkCore;
using CarbonKickers.Domain.Entities;

namespace CarbonKickers.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<TripLog> TripLogs => Set<TripLog>();
    public DbSet<TransportMode> TransportModes => Set<TransportMode>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueParticipant> LeagueParticipants => Set<LeagueParticipant>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Username).IsUnique();
            entity.Property(u => u.Role).HasConversion<string>();
        });

        // Configure TripLog (Relationships with Mode and BaselineMode)
        modelBuilder.Entity<TripLog>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.HasOne<User>()
                  .WithMany(u => u.TripLogs)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.TransportMode)
                  .WithMany()
                  .HasForeignKey(t => t.TransportModeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.BaselineTransportMode)
                  .WithMany()
                  .HasForeignKey(t => t.BaselineTransportModeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Challenge (Challenger and Opponent relations)
        modelBuilder.Entity<Challenge>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Status).HasConversion<string>();

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(c => c.ChallengerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(c => c.OpponentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserBadge (Unique user per badge)
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(ub => ub.Id);
            entity.HasIndex(ub => new { ub.UserId, ub.BadgeId }).IsUnique();

            entity.HasOne<User>()
                  .WithMany(u => u.UserBadges)
                  .HasForeignKey(ub => ub.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Badge>()
                  .WithMany(b => b.UserBadges)
                  .HasForeignKey(ub => ub.BadgeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure LeagueParticipant
        modelBuilder.Entity<LeagueParticipant>(entity =>
        {
            entity.HasKey(lp => lp.Id);
            entity.HasIndex(lp => new { lp.UserId, lp.LeagueId }).IsUnique();

            entity.HasOne<League>()
                  .WithMany(l => l.Participants)
                  .HasForeignKey(lp => lp.LeagueId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<User>()
                  .WithMany(u => u.LeagueParticipants)
                  .HasForeignKey(lp => lp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}