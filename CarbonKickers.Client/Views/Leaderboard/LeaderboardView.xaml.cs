using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Leaderboard;

public partial class LeaderboardView : UserControl
{
    private readonly IShell _shell;
    private WeeklyLeaderboardResponse? _leaderboard;

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
        Brush RowBorder
    );

    private async void LeaderboardView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _leaderboard = await _shell.Api.GetWeeklyLeaderboardAsync();

            ShowRows();
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowError("Unable to load the weekly leaderboard. Please log in again.");
        }
    }

    private void ShowRows()
    {
        if (_leaderboard is null)
        {
            return;
        }

        var entries = _leaderboard.TopEntries.ToList();

        // The signed-in user might rank below the top entries.
        if (_leaderboard.CurrentUser is not null && !entries.Any(entry => entry.IsCurrentUser))
        {
            entries.Add(_leaderboard.CurrentUser);
        }

        var gold = (Brush)FindResource("Brush.Gold");
        var muted = (Brush)FindResource("Brush.GameMuted");
        var deep = (Brush)FindResource("Brush.GameDeep");
        var raised = (Brush)FindResource("Brush.GameRaised");

        RowsItems.ItemsSource = entries
            .OrderBy(entry => entry.Rank)
            .Select(entry => new Row(
                $"#{entry.Rank}",
                entry.Rank <= 3 ? gold : muted,
                entry.IsCurrentUser ? $"{entry.Username} (you)" : entry.Username,
                entry.IsCurrentUser ? FontWeights.Bold : FontWeights.Normal,
                // The weekly endpoint does not return all-time XP tiers.
                "Weekly",
                muted,
                entry.WeeklyXp.ToString("N0"),
                $"{entry.WeeklyCo2SavedKg:F1} kg",
                entry.IsCurrentUser ? raised : deep,
                entry.IsCurrentUser ? gold : Brushes.Transparent
            ))
            .ToList();

        XpHeaderText.Text = "XP this week";
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
