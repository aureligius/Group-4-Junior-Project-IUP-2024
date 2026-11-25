using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.League;

// Preview screen: league members come from PreviewData until the API has league endpoints
public partial class LeagueView : UserControl
{
    private const int PromotionSpots = 3;
    private const int RelegationSpots = 2;

    private readonly IShell _shell;
    private IReadOnlyList<TripHistoryResponse> _trips = [];

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
        Brush DividerBrush);

    public sealed record TierRow(
        string Name,
        FontWeight NameWeight,
        Brush TierBrush,
        string RangeText,
        Visibility PlayerVisibility,
        Brush RowBackground);

    private async void LeagueView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_shell.Player is null)
            {
                await _shell.RefreshPlayerAsync();
            }

            _trips = await _shell.Api.GetTripsAsync();
        }
        catch (Exception)
        {
            // The league still shows its sample players without the player's own numbers
        }

        Show();
    }

    private void Show()
    {
        var me = _shell.Player;
        var myXp = me?.GreenXp ?? 0;
        var tier = PreviewData.TierFor(myXp);
        var next = PreviewData.NextTier(tier);
        var myWeeklyXp = _trips.Where(t => t.LoggedAt >= PreviewData.WeekStart).Sum(t => t.XpEarned);

        LeagueDot.Fill = TierBrush(tier.Name);
        LeagueTitleText.Text = $"{tier.Name.ToUpperInvariant()} LEAGUE";
        DaysLeftText.Text = $"{PreviewData.DaysLeftThisWeek} days left";
        JoinTitleText.Text = $"This week's {tier.Name} league is open.";
        JoinBanner.Visibility = PreviewData.HasJoinedLeague ? Visibility.Collapsed : Visibility.Visible;

        LadderSummaryText.Text = next is null
            ? $"{myXp:N0} Green XP. You're at the top."
            : $"{myXp:N0} Green XP. {next.MinXp - myXp:N0} more to reach {next.Name}.";

        ShowStandings(tier, me?.Username, myWeeklyXp);
        ShowLadder(tier);
    }

    private void ShowStandings(PreviewData.Tier tier, string? myName, int myWeeklyXp)
    {
        var entries = PreviewData.LeagueRivals(tier)
            .Select(r => (Name: r.Username, Xp: r.WeeklyXp, IsYou: false))
            .ToList();

        if (PreviewData.HasJoinedLeague && myName is not null)
        {
            entries.Add((myName, myWeeklyXp, true));
        }

        var ranked = entries.OrderByDescending(e => e.Xp).ToList();
        var gold = (Brush)FindResource("Brush.Gold");
        var coral = (Brush)FindResource("Brush.Coral");
        var muted = (Brush)FindResource("Brush.GameMuted");
        var deep = (Brush)FindResource("Brush.GameDeep");
        var mine = (Brush)FindResource("Brush.GameRaised");
        var gold1 = (Brush)FindResource("Brush.Gold");
        var relegationStart = ranked.Count - RelegationSpots;

        StandingsItems.ItemsSource = ranked
            .Select((e, i) =>
            {
                // Dividers sit above the first row after the promotion zone and above the relegation zone
                var (divider, text, brush) = i == PromotionSpots
                    ? (Visibility.Visible, "Promotion zone above", gold)
                    : i == relegationStart
                        ? (Visibility.Visible, "Relegation zone below", coral)
                        : (Visibility.Collapsed, "", muted);

                return new StandingRow(
                    $"#{i + 1}",
                    i < PromotionSpots ? gold : i >= relegationStart ? coral : muted,
                    e.IsYou ? $"{e.Name} (you)" : e.Name,
                    e.IsYou ? FontWeights.Bold : FontWeights.Normal,
                    e.Xp.ToString("N0"),
                    e.IsYou ? mine : deep,
                    e.IsYou ? gold1 : Brushes.Transparent,
                    divider,
                    text,
                    brush);
            })
            .ToList();
    }

    private void ShowLadder(PreviewData.Tier current)
    {
        var raised = (Brush)FindResource("Brush.GameRaised");
        var deep = (Brush)FindResource("Brush.GameDeep");

        TiersItems.ItemsSource = PreviewData.Tiers
            .Reverse()
            .Select(t => new TierRow(
                t.Name,
                t == current ? FontWeights.Bold : FontWeights.Normal,
                TierBrush(t.Name),
                t.MaxXp is null ? $"{t.MinXp:N0}+ XP" : $"{t.MinXp:N0}–{t.MaxXp:N0} XP",
                t == current ? Visibility.Visible : Visibility.Hidden,
                t == current ? raised : deep))
            .ToList();
    }

    private Brush TierBrush(string tierName) => (Brush)FindResource($"Brush.Tier.{tierName}");

    private void JoinButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewData.JoinLeague();
        Show();
    }
}
