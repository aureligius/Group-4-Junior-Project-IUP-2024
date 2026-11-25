using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.League;

public partial class LeagueView : UserControl
{
    private const int PromotionSpots = 3;
    private const int RelegationSpots = 2;

    private readonly IShell _shell;
    private CurrentLeagueResponse? _league;

    private sealed record Tier(string Name, int MinXp, int? MaxXp);

    private static readonly IReadOnlyList<Tier> Tiers =
    [
        new("Bronze", 0, 99),
        new("Silver", 100, 299),
        new("Gold", 300, 699),
        new("Diamond", 700, null),
    ];

    public LeagueView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += LeagueView_Loaded;
    }

    public sealed record StandingRow(
        string RankText,
        Brush RankBrush,
        string Name,
        FontWeight NameWeight,
        string XpText,
        Brush RowBackground,
        Brush RowBorder,
        Visibility DividerVisibility,
        string DividerText,
        Brush DividerBrush
    );

    public sealed record TierRow(
        string Name,
        FontWeight NameWeight,
        Brush TierBrush,
        string RangeText,
        Visibility PlayerVisibility,
        Brush RowBackground
    );

    private async void LeagueView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _league = await _shell.Api.GetMyLeagueAsync();

            if (_league is null)
            {
                ShowNoLeague(
                    "No weekly league is available yet. Ask an admin to create this week's leagues."
                );
                return;
            }

            ShowLeague();
        }
        catch (HttpRequestException)
        {
            ShowNoLeague("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowNoLeague("Unable to load your weekly league. Please log in again.");
        }
    }

    private void ShowLeague()
    {
        if (_league is null)
        {
            return;
        }

        var league = _league;
        var currentTier = Tiers.FirstOrDefault(t => t.Name == league.TierName) ?? Tiers[0];

        LeagueDot.Fill = TierBrush(league.TierName);
        LeagueTitleText.Text = $"{league.TierName.ToUpperInvariant()} LEAGUE";

        var daysLeft = Math.Max(0, (league.EndDate.Date - DateTime.UtcNow.Date).Days);

        DaysLeftText.Text = $"{daysLeft} day{(daysLeft == 1 ? "" : "s")} left";

        LadderSummaryText.Text =
            $"{league.ParticipantCount} player{(league.ParticipantCount == 1 ? "" : "s")} in your league · "
            + $"tier range: {league.MinXp:N0}–{league.MaxXp:N0} XP";

        ShowStandings(league.Standings);
        ShowLadder(currentTier);

        NoLeagueText.Visibility = Visibility.Collapsed;
    }

    private void ShowStandings(IReadOnlyList<LeagueStandingResponse> standings)
    {
        var gold = (Brush)FindResource("Brush.Gold");
        var coral = (Brush)FindResource("Brush.Coral");
        var muted = (Brush)FindResource("Brush.GameMuted");
        var deep = (Brush)FindResource("Brush.GameDeep");
        var raised = (Brush)FindResource("Brush.GameRaised");

        var relegationStart = Math.Max(PromotionSpots, standings.Count - RelegationSpots);

        StandingsItems.ItemsSource = standings
            .Select(entry =>
            {
                var (divider, text, brush) =
                    entry.Rank == PromotionSpots + 1
                        ? (Visibility.Visible, "Promotion zone above", gold)
                    : entry.Rank == relegationStart + 1 && standings.Count > PromotionSpots
                        ? (Visibility.Visible, "Relegation zone below", coral)
                    : (Visibility.Collapsed, "", muted);

                return new StandingRow(
                    $"#{entry.Rank}",
                    entry.Rank <= PromotionSpots ? gold
                        : entry.Rank > relegationStart ? coral
                        : muted,
                    entry.IsCurrentUser ? $"{entry.Username} (you)" : entry.Username,
                    entry.IsCurrentUser ? FontWeights.Bold : FontWeights.Normal,
                    entry.XpEarned.ToString("N0"),
                    entry.IsCurrentUser ? raised : deep,
                    entry.IsCurrentUser ? gold : Brushes.Transparent,
                    divider,
                    text,
                    brush
                );
            })
            .ToList();
    }

    private void ShowLadder(Tier currentTier)
    {
        var raised = (Brush)FindResource("Brush.GameRaised");
        var deep = (Brush)FindResource("Brush.GameDeep");

        TiersItems.ItemsSource = Tiers
            .Reverse()
            .Select(tier => new TierRow(
                tier.Name,
                tier.Name == currentTier.Name ? FontWeights.Bold : FontWeights.Normal,
                TierBrush(tier.Name),
                tier.MaxXp is null ? $"{tier.MinXp:N0}+ XP" : $"{tier.MinXp:N0}–{tier.MaxXp:N0} XP",
                tier.Name == currentTier.Name ? Visibility.Visible : Visibility.Hidden,
                tier.Name == currentTier.Name ? raised : deep
            ))
            .ToList();
    }

    private Brush TierBrush(string tierName)
    {
        return (Brush)FindResource($"Brush.Tier.{tierName}");
    }

    private void ShowNoLeague(string message)
    {
        NoLeagueText.Text = message;
        NoLeagueText.Visibility = Visibility.Visible;

        StandingsItems.ItemsSource = Array.Empty<StandingRow>();
        LeagueTitleText.Text = "NO ACTIVE LEAGUE";
        DaysLeftText.Text = "";
        LadderSummaryText.Text = "Weekly leagues run from Monday to Sunday.";
    }
}
