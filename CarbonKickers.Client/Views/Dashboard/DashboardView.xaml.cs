using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Dashboard;

public partial class DashboardView : UserControl
{
    // The tallest bar uses this share of the chart height, leaving room for its value label
    private const double MaxBarShare = 0.85;

    private readonly IShell _shell;

    public DashboardView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += DashboardView_Loaded;
    }

    // One bar in the 7-day chart
    public sealed record DayBar(
        string DayLabel,
        GridLength BarFill,
        GridLength BarRest,
        string ValueLabel,
        Visibility ValueVisibility,
        string Tooltip);

    private async void DashboardView_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateCompetePanel(_shell.Player?.GreenXp ?? 0);
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

            ShowTier(dashboard.GreenXp);
            ShowWeek(dashboard.LastSevenDays);
            UpdateCompetePanel(dashboard.GreenXp);
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

    private async Task LoadBadgesAsync()
    {
        try
        {
            var badges = await _shell.Api.GetBadgesAsync();
            LatestBadgesItems.ItemsSource = badges.Take(3).ToList();
            // Up to three medals; the rest are counted, the details live in the tooltip
            BadgesSummaryText.Text = badges.Count > 3 ? $"+{badges.Count - 3}" : "";
            LatestBadgesPanel.ToolTip = badges.Count == 0
                ? null
                : $"{badges.Count} unlocked · latest: {badges[0].Title}";
            LatestBadgesPanel.Visibility = badges.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            NoBadgesText.Visibility = badges.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (Exception)
        {
            NoBadgesText.Text = "Badges couldn't load.";
            NoBadgesText.Visibility = Visibility.Visible;
        }
    }

    private void ShowTier(int greenXp)
    {
        var tier = PreviewData.TierFor(greenXp);
        var next = PreviewData.NextTier(tier);

        TierText.Text = $"{tier.Name} league";
        TierDot.Fill = (Brush)FindResource($"Brush.Tier.{tier.Name}");

        double progress;
        if (next is null)
        {
            NextTierText.Text = "Top tier reached";
            progress = 100;
        }
        else
        {
            NextTierText.Text = $"{next.MinXp - greenXp:N0} XP to {next.Name}";
            progress = 100.0 * (greenXp - tier.MinXp) / (next.MinXp - tier.MinXp);
        }

        // Signature moment: the XP bar fills up to where the player stands
        // Instant when Windows animations are turned off
        var duration = SystemParameters.ClientAreaAnimation ? TimeSpan.FromMilliseconds(700) : TimeSpan.Zero;
        XpProgress.BeginAnimation(
            RangeBase.ValueProperty,
            new DoubleAnimation(0, progress, duration)
            {
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 },
            });
    }

    private void ShowWeek(IReadOnlyList<DailyImpactResponse> days)
    {
        var max = days.Count == 0 ? 0 : days.Max(d => d.Co2SavedKg);
        var total = days.Sum(d => d.Co2SavedKg);

        WeekTotalText.Text = $"{total:F2} kg this week";
        EmptyWeekText.Visibility = max <= 0 ? Visibility.Visible : Visibility.Collapsed;

        // Only the best day carries a number; the rest are in the tooltips
        DailyImpactItems.ItemsSource = days
            .Select(day =>
            {
                var share = max <= 0 || day.Co2SavedKg <= 0 ? 0 : Math.Max(0.02, MaxBarShare * day.Co2SavedKg / max);
                return (day, share);
            })
            .Select(x => new DayBar(
                x.day.Date.ToString("ddd"),
                new GridLength(x.share, GridUnitType.Star),
                new GridLength(1 - x.share, GridUnitType.Star),
                $"{x.day.Co2SavedKg:F2}",
                max > 0 && x.day.Co2SavedKg == max ? Visibility.Visible : Visibility.Collapsed,
                $"{x.day.Date:dddd d MMM}: {x.day.Co2SavedKg:F2} kg CO₂ kicked from {x.day.TripCount} trip(s)"))
            .ToList();
    }

    private void UpdateCompetePanel(int greenXp)
    {
        var tier = PreviewData.TierFor(greenXp);
        var rank = 1 + PreviewData.Rivals.Count(r => r.GreenXp > greenXp);
        var invites = PreviewData.Challenges.Count(c => c.State == PreviewData.ChallengeState.Incoming);
        var battles = PreviewData.Challenges.Count(c => c.State == PreviewData.ChallengeState.Active);

        if (PreviewData.HasJoinedCompetition)
        {
            CompeteStatusText.Text = $"You're competing · rank #{rank} of {PreviewData.Rivals.Count + 1}";
            JoinCompetitionButton.Visibility = Visibility.Collapsed;
            LeaderboardTileText.Text = $"You're #{rank} all time";
        }
        else
        {
            CompeteStatusText.Text = "Get ranked and battle other players.";
            JoinCompetitionButton.Visibility = Visibility.Visible;
            LeaderboardTileText.Text = "See who's kicking the most CO₂";
        }

        if (PreviewData.HasJoinedLeague)
        {
            LeagueTileTitle.Text = "Weekly league";
            LeagueTileText.Text = $"{tier.Name} league · {PreviewData.DaysLeftThisWeek} days left";
        }
        else
        {
            LeagueTileTitle.Text = "Join weekly league";
            LeagueTileText.Text = $"This week's {tier.Name} league is open";
        }

        ArenaTileText.Text = invites > 0
            ? $"{invites} invite waiting · {battles} battles on"
            : $"{battles} battles on";
    }

    private void ShowError(string message)
    {
        DashboardErrorText.Text = message;
        DashboardErrorText.Visibility = Visibility.Visible;
    }

    private void JoinCompetitionButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewData.JoinCompetition();
        UpdateCompetePanel(_shell.Player?.GreenXp ?? 0);
    }

    private void LeagueTile_Click(object sender, RoutedEventArgs e)
    {
        PreviewData.JoinLeague();
        _shell.Navigate(Screen.League);
    }

    private void LogTripButton_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.LogTrip);

    private void TripHistoryButton_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.History);

    private void BadgesButton_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.Badges);

    private void LeaderboardTile_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.Leaderboard);

    private void ArenaTile_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.Arena);
}
