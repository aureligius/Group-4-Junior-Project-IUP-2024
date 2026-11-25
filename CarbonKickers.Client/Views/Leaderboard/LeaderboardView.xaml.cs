using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Leaderboard;

// Preview screen: rivals come from PreviewData until the API has a leaderboard endpoint
public partial class LeaderboardView : UserControl
{
    private readonly IShell _shell;
    private IReadOnlyList<TripHistoryResponse> _trips = [];

    public LeaderboardView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += LeaderboardView_Loaded;
    }

    public sealed record Row(
        string RankText,
        Brush RankBrush,
        string Name,
        FontWeight NameWeight,
        string TierName,
        Brush TierBrush,
        string XpText,
        string Co2Text,
        Brush RowBackground,
        Brush RowBorder);

    private async void LeaderboardView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_shell.Player is null)
            {
                await _shell.RefreshPlayerAsync();
            }

            // Your weekly XP is real: the XP from your trips in the last 7 days
            _trips = await _shell.Api.GetTripsAsync();
        }
        catch (Exception)
        {
            ErrorText.Text = "Couldn't load your own numbers, so the board shows sample players only.";
            ErrorText.Visibility = Visibility.Visible;
        }

        ShowRows();
    }

    private void ShowRows()
    {
        var weekly = WeekTab.IsChecked == true;
        var me = _shell.Player;
        var myWeeklyXp = _trips.Where(t => t.LoggedAt >= DateTime.Now.AddDays(-7)).Sum(t => t.XpEarned);

        var entries = PreviewData.Rivals
            .Select(r => (Name: r.Username, AllTime: r.GreenXp, Week: r.WeeklyXp, r.Co2SavedKg, IsYou: false))
            .ToList();

        if (PreviewData.HasJoinedCompetition && me is not null)
        {
            entries.Add((me.Username, me.GreenXp, myWeeklyXp, me.TotalCo2SavedKg, true));
        }

        var ranked = entries.OrderByDescending(e => weekly ? e.Week : e.AllTime).ToList();

        var gold = (Brush)FindResource("Brush.Gold");
        var muted = (Brush)FindResource("Brush.GameMuted");
        var deep = (Brush)FindResource("Brush.GameDeep");
        var mine = (Brush)FindResource("Brush.GameRaised");
        var gold1 = (Brush)FindResource("Brush.Gold");

        RowsItems.ItemsSource = ranked
            .Select((e, i) =>
            {
                var tier = PreviewData.TierFor(e.AllTime);
                return new Row(
                    $"#{i + 1}",
                    i < 3 ? gold : muted,
                    e.IsYou ? $"{e.Name} (you)" : e.Name,
                    e.IsYou ? FontWeights.Bold : FontWeights.Normal,
                    tier.Name,
                    (Brush)FindResource($"Brush.Tier.{tier.Name}"),
                    (weekly ? e.Week : e.AllTime).ToString("N0"),
                    $"{e.Co2SavedKg:F1} kg",
                    e.IsYou ? mine : deep,
                    e.IsYou ? gold1 : Brushes.Transparent);
            })
            .ToList();

        XpHeaderText.Text = weekly ? "XP this week" : "Green XP";
        JoinBanner.Visibility = PreviewData.HasJoinedCompetition ? Visibility.Collapsed : Visibility.Visible;
    }

    private void PeriodTab_Checked(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            ShowRows();
        }
    }

    private void JoinButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewData.JoinCompetition();
        ShowRows();
    }
}
