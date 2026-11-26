using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Dashboard;

public partial class DashboardView : UserControl
{
    private const double MaxBarShare = 0.85;
    private readonly IShell _shell;

    public DashboardView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += DashboardView_Loaded;
    }

    public sealed record DayBar(
        string DayLabel,
        GridLength BarFill,
        GridLength BarRest,
        string ValueLabel,
        Visibility ValueVisibility,
        string Tooltip
    );

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var dashboard = await _shell.Api.GetDashboardAsync();
            _shell.SetPlayer(dashboard);

            WelcomeText.Text = $"Welcome back, {dashboard.Username}";
            PlayerNameText.Text = dashboard.Username;
            GreenXpText.Text = dashboard.GreenXp.ToString("N0");
            Co2SavedText.Text = $"{dashboard.TotalCo2SavedKg:F2} kg";
            TotalTripsText.Text = dashboard.TotalTrips.ToString("N0");

            ShowWeek(dashboard.LastSevenDays);

            await LoadGamificationSummaryAsync(dashboard);

        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowError("Unable to load your dashboard. Please log in again.");
        }

        await LoadBadgesAsync();
    }

    private async Task LoadGamificationSummaryAsync(DashboardResponse dashboard)
    {
        await LoadLeaderboardSummaryAsync();
        await LoadLeagueSummaryAsync(dashboard.GreenXp);
        await LoadArenaSummaryAsync();
    }

    private async Task LoadLeaderboardSummaryAsync()
    {
        try
        {
            var leaderboard = await _shell.Api.GetWeeklyLeaderboardAsync();


            if (leaderboard.CurrentUser is null)
            {
                CompeteStatusText.Text = "Log a greener trip this week to enter the rankings.";
                LeaderboardTileText.Text = "No weekly score yet";
                return;
            }

            var entry = leaderboard.CurrentUser;

            CompeteStatusText.Text =
                $"You're ranked #{entry.Rank} this week · {entry.WeeklyCo2SavedKg:F1} kg saved";

            LeaderboardTileText.Text = $"You're #{entry.Rank} this week · {entry.WeeklyXp:N0} XP";
        }
        catch
        {
            CompeteStatusText.Text = "Leaderboard unavailable right now.";
            LeaderboardTileText.Text = "View this week's ranking";
        }
    }

    private async Task LoadLeagueSummaryAsync(int greenXp)
    {
        try
        {
            var league = await _shell.Api.GetMyLeagueAsync();

            ShowTier(greenXp, league);

            if (league is null)
            {
                LeagueTileTitle.Text = "Weekly league";
                LeagueTileText.Text = "No active league yet";
                return;
            }

            var daysLeft = Math.Max(0, (league.EndDate.Date - DateTime.UtcNow.Date).Days);

            LeagueTileTitle.Text = "Weekly league";
            LeagueTileText.Text =
                $"{league.TierName} league · {daysLeft} day{(daysLeft == 1 ? "" : "s")} left";
        }
        catch
        {
            ShowTier(greenXp, null);
            LeagueTileTitle.Text = "Weekly league";
            LeagueTileText.Text = "League unavailable right now";
        }
    }

    private async Task LoadArenaSummaryAsync()
    {
        try
        {
            var challenges = await _shell.Api.GetChallengesAsync();

            var invites = challenges.Count(challenge =>
                challenge.Status == "Pending" && !challenge.IsCurrentUserChallenger
            );

            var battles = challenges.Count(challenge => challenge.Status == "Accepted");

            ArenaTileText.Text =
                invites > 0
                    ? $"{invites} invite{(invites == 1 ? "" : "s")} waiting · {battles} battle{(battles == 1 ? "" : "s")} on"
                    : $"{battles} battle{(battles == 1 ? "" : "s")} on";
        }
        catch
        {
            ArenaTileText.Text = "Challenge another player";
        }
    }

    private async Task LoadBadgesAsync()
    {
        try
        {
            var badges = await _shell.Api.GetBadgesAsync();

            LatestBadgesItems.ItemsSource = badges.Take(3).ToList();
            BadgesSummaryText.Text = badges.Count > 3 ? $"+{badges.Count - 3}" : "";

            LatestBadgesPanel.ToolTip =
                badges.Count == 0 ? null : $"{badges.Count} unlocked · latest: {badges[0].Title}";

            LatestBadgesPanel.Visibility =
                badges.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            NoBadgesText.Visibility = badges.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            NoBadgesText.Text = "Badges couldn't load.";
            NoBadgesText.Visibility = Visibility.Visible;
        }
    }

    private void ShowTier(int greenXp, CurrentLeagueResponse? league)
    {
        if (league is null)
        {
            TierText.Text = "No active league";
            TierDot.Fill = (Brush)FindResource("Brush.Tier.Bronze");
            NextTierText.Text = "Weekly leagues have not been created yet.";

            XpProgress.BeginAnimation(
                RangeBase.ValueProperty,
                new DoubleAnimation(0, 0, TimeSpan.Zero)
            );

            return;
        }

        TierText.Text = $"{league.TierName} league";
        TierDot.Fill = (Brush)FindResource($"Brush.Tier.{league.TierName}");

        double progress;
        string progressText;

        if (league.MaxXp == int.MaxValue)
        {
            progress = 100;
            progressText = "Top tier reached";
        }
        else
        {
            progress = Math.Clamp(
                100.0 * (greenXp - league.MinXp) / (league.MaxXp - league.MinXp + 1),
                0,
                100
            );

            progressText =
                greenXp > league.MaxXp
                    ? "Your league changes next week."
                    : $"{league.MaxXp - greenXp + 1:N0} XP until next tier";
        }

        NextTierText.Text = progressText;

        var duration = SystemParameters.ClientAreaAnimation
            ? TimeSpan.FromMilliseconds(700)
            : TimeSpan.Zero;

        XpProgress.BeginAnimation(
            RangeBase.ValueProperty,
            new DoubleAnimation(0, progress, duration)
            {
                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 5,
                },
            }
        );
    }

    private void ShowWeek(IReadOnlyList<DailyImpactResponse> days)
    {
        var max = days.Count == 0 ? 0 : days.Max(day => day.Co2SavedKg);
        var total = days.Sum(day => day.Co2SavedKg);

        WeekTotalText.Text = $"{total:F2} kg this week";
        EmptyWeekText.Visibility = max <= 0 ? Visibility.Visible : Visibility.Collapsed;

        DailyImpactItems.ItemsSource = days.Select(day =>
            {
                var share =
                    max <= 0 || day.Co2SavedKg <= 0
                        ? 0
                        : Math.Max(0.02, MaxBarShare * day.Co2SavedKg / max);

                return (day, share);
            })
            .Select(item => new DayBar(
                item.day.Date.ToString("ddd"),
                new GridLength(item.share, GridUnitType.Star),
                new GridLength(1 - item.share, GridUnitType.Star),
                $"{item.day.Co2SavedKg:F2}",
                max > 0 && item.day.Co2SavedKg == max ? Visibility.Visible : Visibility.Collapsed,
                $"{item.day.Date:dddd d MMM}: {item.day.Co2SavedKg:F2} kg CO₂ kicked from {item.day.TripCount} trip(s)"
            ))
            .ToList();
    }

    private void ShowError(string message)
    {
        DashboardErrorText.Text = message;
        DashboardErrorText.Visibility = Visibility.Visible;
    }

    private void LeagueTile_Click(object sender, RoutedEventArgs e)
    {
        _shell.Navigate(Screen.League);
    }

    private void LogTripButton_Click(object sender, RoutedEventArgs e) =>
        _shell.Navigate(Screen.LogTrip);

    private void TripHistoryButton_Click(object sender, RoutedEventArgs e) =>
        _shell.Navigate(Screen.History);

    private void BadgesButton_Click(object sender, RoutedEventArgs e) =>
        _shell.Navigate(Screen.Badges);

    private void LeaderboardTile_Click(object sender, RoutedEventArgs e) =>
        _shell.Navigate(Screen.Leaderboard);

    private void ArenaTile_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.Arena);
}
