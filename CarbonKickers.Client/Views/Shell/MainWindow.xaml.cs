using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Arena;
using CarbonKickers.Client.Views.Authentication;
using CarbonKickers.Client.Views.Badges;
using CarbonKickers.Client.Views.Dashboard;
using CarbonKickers.Client.Views.League;
using CarbonKickers.Client.Views.Leaderboard;
using CarbonKickers.Client.Views.Trips;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Shell;

// The one main window: sidebar on the left, the current screen on the right
public partial class MainWindow : Window, IShell
{
    private bool _isSyncingNav;

    public MainWindow(ApiClient apiClient)
    {
        Api = apiClient;

        InitializeComponent();

        Loaded += (_, _) => Navigate(Screen.Dashboard);

        Api.SessionExpired += Api_SessionExpired;
        Closed += (_, _) => Api.SessionExpired -= Api_SessionExpired;
    }

    // The login token lasts 8 hours; when the API rejects it, go back to login instead of
    // leaving every screen showing a load error
    private void Api_SessionExpired(object? sender, EventArgs e)
    {
        new LoginWindow("Your session has expired. Please log in again.").Show();
        Close();
    }

    public ApiClient Api { get; }

    public DashboardResponse? Player { get; private set; }

    public void Navigate(Screen screen)
    {
        UserControl view = screen switch
        {
            Screen.Dashboard => new DashboardView(this),
            Screen.LogTrip => new LogTripView(this),
            Screen.History => new TripHistoryView(this),
            Screen.Badges => new BadgesView(this),
            Screen.Leaderboard => new LeaderboardView(this),
            Screen.League => new LeagueView(this),
            Screen.Arena => new ArenaView(this),
            _ => throw new ArgumentOutOfRangeException(nameof(screen)),
        };

        ScreenHost.Content = view;
        SyncNav(screen);
    }

    public void SetPlayer(DashboardResponse player)
    {
        Player = player;
        PlayerNameText.Text = player.Username;
        PlayerXpText.Text = $"{player.GreenXp:N0} XP";
    }

    public async Task<DashboardResponse?> RefreshPlayerAsync()
    {
        try
        {
            SetPlayer(await Api.GetDashboardAsync());
        }
        catch (Exception)
        {
            // Each screen shows its own error; the sidebar just keeps its last values
        }

        return Player;
    }

    private void SyncNav(Screen screen)
    {
        _isSyncingNav = true;
        var item = screen switch
        {
            Screen.Dashboard => NavDashboard,
            Screen.LogTrip => NavLogTrip,
            Screen.History => NavHistory,
            Screen.Badges => NavBadges,
            Screen.Leaderboard => NavLeaderboard,
            Screen.League => NavLeague,
            _ => NavArena,
        };
        item.IsChecked = true;
        _isSyncingNav = false;
    }

    private void NavItem_Checked(object sender, RoutedEventArgs e)
    {
        if (_isSyncingNav || !IsLoaded)
        {
            return;
        }

        Screen screen = sender switch
        {
            _ when sender == NavDashboard => Screen.Dashboard,
            _ when sender == NavLogTrip => Screen.LogTrip,
            _ when sender == NavHistory => Screen.History,
            _ when sender == NavBadges => Screen.Badges,
            _ when sender == NavLeaderboard => Screen.Leaderboard,
            _ when sender == NavLeague => Screen.League,
            _ => Screen.Arena,
        };
        Navigate(screen);
    }

    private void LogOutButton_Click(object sender, RoutedEventArgs e)
    {
        new LoginWindow().Show();
        Close();
    }
}
