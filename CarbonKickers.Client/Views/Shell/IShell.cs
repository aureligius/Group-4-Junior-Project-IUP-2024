using CarbonKickers.Client.Services;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Shell;


public enum Screen
{
    Dashboard,
    LogTrip,
    History,
    Badges,
    Leaderboard,
    League,
    Arena,
    Admin,
}

// What every screen inside the main window can ask of it
public interface IShell
{
    ApiClient Api { get; }

    // Latest dashboard data for the signed-in player (null until it first loads)
    DashboardResponse? Player { get; }

    void Navigate(Screen screen);

    // Shows fresh player data in the sidebar
    void SetPlayer(DashboardResponse player);

    // Reloads the player's XP and totals quietly, e.g. after a trip is saved
    Task<DashboardResponse?> RefreshPlayerAsync();
}
