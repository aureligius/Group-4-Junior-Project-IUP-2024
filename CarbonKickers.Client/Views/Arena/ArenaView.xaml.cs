using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Arena;

public partial class ArenaView : UserControl
{
    private readonly IShell _shell;
    private IReadOnlyList<ChallengeResponse> _challenges = [];
    private int _searchVersion;
    private UserSearchResponse? _selectedOpponent;
    private bool _isSelectingOpponent;

    public ArenaView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += ArenaView_Loaded;
    }

    public sealed record IncomingChallengeRow(Guid ChallengeId, string Opponent, int Days);

    public sealed record WaitingChallengeRow(string Opponent, int Days);

    public sealed record BattleRow(
        string Title,
        string Opponent,
        string StatusText,
        Brush StatusBrush,
        double MinePercent,
        double TheirsPercent,
        string MineText,
        string TheirsText
    );

    public sealed record ResultRow(
        string Title,
        string DetailText,
        string ResultText,
        Brush ResultBackground
    );

    private async void ArenaView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadChallengesAsync();
    }

    private async Task LoadChallengesAsync()
    {
        try
        {
            _challenges = await _shell.Api.GetChallengesAsync();
            ShowChallenges();
        }
        catch (HttpRequestException)
        {
            ShowStatus("Cannot reach the API. Start CarbonKickers.Api first.", true);
        }
        catch (Exception)
        {
            ShowStatus("Unable to load challenges. Please log in again.", true);
        }
    }

    private void ShowChallenges()
    {
        var gold = (Brush)FindResource("Brush.Gold");
        var coral = (Brush)FindResource("Brush.Coral");
        var muted = (Brush)FindResource("Brush.GameMuted");

        var incoming = _challenges
            .Where(challenge => challenge.Status == "Pending" && !challenge.IsCurrentUserChallenger)
            .Select(challenge => new IncomingChallengeRow(
                challenge.Id,
                OpponentName(challenge),
                DaysBetween(challenge)
            ))
            .ToList();

        IncomingItems.ItemsSource = incoming;
        IncomingPanel.Visibility = incoming.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        var active = _challenges
            .Where(challenge => challenge.Status == "Accepted")
            .OrderBy(challenge => challenge.EndDate)
            .Select(challenge =>
            {
                var mine = MySavedKg(challenge);
                var theirs = OpponentSavedKg(challenge);
                var total = Math.Max(Math.Max(mine, theirs), 0.1);

                var (status, brush) =
                    mine > theirs ? ("You're ahead", gold)
                    : mine < theirs ? ("You're behind", coral)
                    : ("Tied", muted);

                var daysLeft = Math.Max(0, (challenge.EndDate.Date - DateTime.UtcNow.Date).Days);

                return new BattleRow(
                    $"You vs {OpponentName(challenge)}",
                    OpponentName(challenge),
                    $"{status} · {daysLeft} day{(daysLeft == 1 ? "" : "s")} left",
                    brush,
                    100 * mine / total,
                    100 * theirs / total,
                    $"{mine:F1} kg",
                    $"{theirs:F1} kg"
                );
            })
            .ToList();

        ActiveItems.ItemsSource = active;
        NoBattlesText.Visibility = active.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        var waiting = _challenges
            .Where(challenge => challenge.Status == "Pending" && challenge.IsCurrentUserChallenger)
            .Select(challenge => new WaitingChallengeRow(
                OpponentName(challenge),
                DaysBetween(challenge)
            ))
            .ToList();

        WaitingItems.ItemsSource = waiting;
        WaitingPanel.Visibility = waiting.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        PastItems.ItemsSource = _challenges
            .Where(challenge => challenge.Status is "Completed" or "Declined" or "Expired")
            .OrderByDescending(challenge => challenge.EndDate)
            .Select(challenge =>
            {
                var result = ResultFor(challenge);

                return new ResultRow(
                    $"vs {OpponentName(challenge)}",
                    $"You {MySavedKg(challenge):F1} kg · them {OpponentSavedKg(challenge):F1} kg · ended {challenge.EndDate.ToLocalTime():d MMM}",
                    result.Text,
                    result.Brush
                );
            })
            .ToList();
    }

    private async void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: IncomingChallengeRow challenge })
        {
            return;
        }

        try
        {
            await _shell.Api.AcceptChallengeAsync(challenge.ChallengeId);
            ShowStatus("Challenge accepted. Start logging greener trips!", false);
            await LoadChallengesAsync();
        }
        catch (Exception exception)
        {
            ShowStatus(exception.Message, true);
        }
    }

    private async void DeclineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: IncomingChallengeRow challenge })
        {
            return;
        }

        try
        {
            await _shell.Api.DeclineChallengeAsync(challenge.ChallengeId);
            ShowStatus("Challenge declined.", false);
            await LoadChallengesAsync();
        }
        catch (Exception exception)
        {
            ShowStatus(exception.Message, true);
        }
    }

    private async void OpponentSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSelectingOpponent)
        {
            return;
        }

        _selectedOpponent = null;
        SendChallengeButton.IsEnabled = false;

        var query = OpponentSearchTextBox.Text.Trim();

        if (query.Length < 2)
        {
            OpponentResultsListBox.ItemsSource = null;
            OpponentResultsListBox.Visibility = Visibility.Collapsed;
            return;
        }

        var searchVersion = ++_searchVersion;

        try
        {
            var users = await _shell.Api.SearchUsersAsync(query);

            if (searchVersion != _searchVersion)
            {
                return;
            }

            OpponentResultsListBox.ItemsSource = users;
            OpponentResultsListBox.Visibility =
                users.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch
        {
            OpponentResultsListBox.Visibility = Visibility.Collapsed;
        }
    }

    private void OpponentResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OpponentResultsListBox.SelectedItem is not UserSearchResponse opponent)
        {
            return;
        }

        _isSelectingOpponent = true;

        _selectedOpponent = opponent;
        OpponentSearchTextBox.Text = opponent.Username;
        OpponentResultsListBox.Visibility = Visibility.Collapsed;
        SendChallengeButton.IsEnabled = true;

        _isSelectingOpponent = false;
    }

    private async void SendChallengeButton_Click(object sender, RoutedEventArgs e)
    {
        if (
            _selectedOpponent is not UserSearchResponse opponent
            || LengthComboBox.SelectedItem is not ComboBoxItem { Tag: string daysText }
            || !int.TryParse(daysText, out var days)
        )
        {
            ShowStatus("Select a player and a battle length first.", true);
            return;
        }

        try
        {
            // The backend requires a future start time.
            var startDate = DateTimeOffset.UtcNow.AddMinutes(2);

            await _shell.Api.CreateChallengeAsync(
                new CreateChallengeRequest
                {
                    OpponentUsername = opponent.Username,
                    StartDate = startDate,
                    EndDate = startDate.AddDays(days),
                }
            );

            _selectedOpponent = null;
            OpponentSearchTextBox.Clear();
            OpponentResultsListBox.ItemsSource = null;
            OpponentResultsListBox.Visibility = Visibility.Collapsed;

            ShowStatus(
                $"Challenge sent to {opponent.Username}. It is waiting for their reply.",
                false
            );

            await LoadChallengesAsync();
        }
        catch (Exception exception)
        {
            ShowStatus(exception.Message, true);
        }
    }

    private static string OpponentName(ChallengeResponse challenge)
    {
        return challenge.IsCurrentUserChallenger
            ? challenge.OpponentUsername
            : challenge.ChallengerUsername;
    }

    private static double MySavedKg(ChallengeResponse challenge)
    {
        return challenge.IsCurrentUserChallenger
            ? challenge.ChallengerSavedKg
            : challenge.OpponentSavedKg;
    }

    private static double OpponentSavedKg(ChallengeResponse challenge)
    {
        return challenge.IsCurrentUserChallenger
            ? challenge.OpponentSavedKg
            : challenge.ChallengerSavedKg;
    }

    private static int DaysBetween(ChallengeResponse challenge)
    {
        return Math.Max(1, (int)Math.Ceiling((challenge.EndDate - challenge.StartDate).TotalDays));
    }

    private (string Text, Brush Brush) ResultFor(ChallengeResponse challenge)
    {
        var gold = (Brush)FindResource("Brush.Gold");
        var coral = (Brush)FindResource("Brush.Coral");
        var muted = (Brush)FindResource("Brush.GameMuted");

        if (challenge.Status == "Completed")
        {
            var iWon = challenge.IsCurrentUserChallenger
                ? challenge.WinnerUsername == challenge.ChallengerUsername
                : challenge.WinnerUsername == challenge.OpponentUsername;

            return iWon ? ("WON", gold) : ("LOST", coral);
        }

        return challenge.Status == "Declined" ? ("DECLINED", muted) : ("EXPIRED", coral);
    }

    private void ShowStatus(string message, bool isError)
    {
        SendStatusText.Text = message;
        SendStatusText.Foreground = isError
            ? (Brush)FindResource("Brush.Danger")
            : (Brush)FindResource("Brush.GameMuted");
        SendStatusText.Visibility = Visibility.Visible;
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await LoadChallengesAsync();
        ShowStatus("Arena updated.", false);
    }
}
