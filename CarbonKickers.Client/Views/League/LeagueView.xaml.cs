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

    private IReadOnlyList<LeagueTierResponse> _tiers = [];

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
            _tiers = await _shell.Api.GetLeagueTiersAsync();
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
        var currentTier = _tiers.FirstOrDefault(tier => tier.TierName == league.TierName);

        if (currentTier is null)
        {
            ShowNoLeague("The API returned an unknown league tier.");
            return;
        }

        LeagueDot.Fill = TierBrush(league.TierName);
        LeagueTitleText.Text = $"{league.TierName.ToUpperInvariant()} LEAGUE";

        var daysLeft = Math.Max(0, (league.EndDate.Date - DateTime.UtcNow.Date).Days);

        DaysLeftText.Text = $"{daysLeft} day{(daysLeft == 1 ? "" : "s")} left";

        var tierRange = currentTier.MaxXp is null
            ? $"{currentTier.MinXp:N0}+ XP"
            : $"{currentTier.MinXp:N0}–{currentTier.MaxXp:N0} XP";

        LadderSummaryText.Text =
            $"{league.ParticipantCount} player{(league.ParticipantCount == 1 ? "" : "s")} in your league · "
            + $"tier range: {tierRange}";

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

    private void ShowLadder(LeagueTierResponse? currentTier)
{
    var raised = (Brush)FindResource("Brush.GameRaised");
    var deep = (Brush)FindResource("Brush.GameDeep");

    TiersItems.ItemsSource = _tiers
        .Reverse()
        .Select(tier => new TierRow(
            tier.TierName,
            tier.TierName == currentTier?.TierName
                ? FontWeights.Bold
                : FontWeights.Normal,
            TierBrush(tier.TierName),
            tier.MaxXp is null
                ? $"{tier.MinXp:N0}+ XP"
                : $"{tier.MinXp:N0}–{tier.MaxXp:N0} XP",
            tier.TierName == currentTier?.TierName
                ? Visibility.Visible
                : Visibility.Hidden,
            tier.TierName == currentTier?.TierName
                ? raised
                : deep
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
        ShowLadder(null);
        LeagueTitleText.Text = "NO ACTIVE LEAGUE";
        DaysLeftText.Text = "";
        LadderSummaryText.Text = "Weekly leagues run from Monday to Sunday.";
    }
}
