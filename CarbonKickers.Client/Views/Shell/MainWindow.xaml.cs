using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Admin;
using CarbonKickers.Client.Views.Arena;
using CarbonKickers.Client.Views.Authentication;
using CarbonKickers.Client.Views.Badges;
using CarbonKickers.Client.Views.Dashboard;
using CarbonKickers.Client.Views.Leaderboard;
using CarbonKickers.Client.Views.League;
using CarbonKickers.Client.Views.Trips;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Shell;

// The one main window: sidebar on the left, the current screen on the right
public partial class MainWindow : Window, IShell
{
    private bool _isSyncingNav;
    private readonly bool _isAdmin;

    public MainWindow(ApiClient apiClient, LoginResponse loginResponse)
    {
        Api = apiClient;

        _isAdmin = string.Equals(loginResponse.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        InitializeComponent();

        AdminNavSection.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;

        Loaded += (_, _) => Navigate(Screen.Dashboard);
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
            Screen.Admin when _isAdmin => new AdminView(this),
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
            Screen.Admin => NavAdmin,
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
            _ when sender == NavAdmin => Screen.Admin,
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
