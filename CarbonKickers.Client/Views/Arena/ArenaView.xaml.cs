using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using static CarbonKickers.Client.Services.PreviewData;

namespace CarbonKickers.Client.Views.Arena;

// Preview screen: challenges come from PreviewData until the API has arena endpoints
public partial class ArenaView : UserControl
{
    public ArenaView(IShell shell)
    {
        InitializeComponent();

        Loaded += (_, _) => Show();
    }

    public sealed record BattleRow(
        string Title,
        string Opponent,
        string StatusText,
        Brush StatusBrush,
        double MinePercent,
        double TheirsPercent,
        string MineText,
        string TheirsText);

    public sealed record ResultRow(string Title, string DetailText, string ResultText, Brush ResultBackground);

    private void Show()
    {
        var gold = (Brush)FindResource("Brush.Gold");
        var coral = (Brush)FindResource("Brush.Coral");
        var muted = (Brush)FindResource("Brush.GameMuted");

        var incoming = Challenges.Where(c => c.State == ChallengeState.Incoming).ToList();
        IncomingItems.ItemsSource = incoming;
        IncomingPanel.Visibility = incoming.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        var active = Challenges
            .Where(c => c.State == ChallengeState.Active)
            .OrderBy(c => c.EndsAt)
            .Select(c =>
            {
                var top = Math.Max(Math.Max(c.MineKg, c.TheirsKg), 0.1);
                var daysLeft = Math.Max(0, (c.EndsAt.Date - DateTime.Today).Days);
                var (status, brush) = c.MineKg > c.TheirsKg ? ("You're ahead", gold)
                    : c.MineKg < c.TheirsKg ? ("You're behind", coral)
                    : ("Tied", muted);
                return new BattleRow(
                    $"You vs {c.Opponent}",
                    c.Opponent,
                    $"{status} · {daysLeft} days left",
                    brush,
                    100 * c.MineKg / top,
                    100 * c.TheirsKg / top,
                    $"{c.MineKg:F1} kg",
                    $"{c.TheirsKg:F1} kg");
            })
            .ToList();
        ActiveItems.ItemsSource = active;
        NoBattlesText.Visibility = active.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        PastItems.ItemsSource = Challenges
            .Where(c => c.State is ChallengeState.Won or ChallengeState.Lost)
            .OrderByDescending(c => c.EndsAt)
            .Select(c => new ResultRow(
                $"vs {c.Opponent}",
                $"You {c.MineKg:F1} kg · them {c.TheirsKg:F1} kg · ended {c.EndsAt:d MMM}",
                c.State == ChallengeState.Won ? "WON" : "LOST",
                c.State == ChallengeState.Won ? gold : coral))
            .ToList();

        var waiting = Challenges.Where(c => c.State == ChallengeState.Waiting).ToList();
        WaitingItems.ItemsSource = waiting;
        WaitingPanel.Visibility = waiting.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        var opponents = ChallengeableRivals().ToList();
        OpponentComboBox.ItemsSource = opponents;
        OpponentComboBox.SelectedIndex = opponents.Count > 0 ? 0 : -1;
        SendChallengeButton.IsEnabled = opponents.Count > 0;
    }

    private void AcceptButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Challenge challenge })
        {
            Accept(challenge);
            Show();
        }
    }

    private void DeclineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Challenge challenge })
        {
            Decline(challenge);
            Show();
        }
    }

    private void SendChallengeButton_Click(object sender, RoutedEventArgs e)
    {
        if (OpponentComboBox.SelectedItem is not string opponent
            || LengthComboBox.SelectedItem is not ComboBoxItem { Tag: string days })
        {
            return;
        }

        SendChallenge(opponent, int.Parse(days));
        Show();

        SendStatusText.Text = $"Challenge sent to {opponent}. It shows under Awaiting reply.";
        SendStatusText.Visibility = Visibility.Visible;
    }
}
