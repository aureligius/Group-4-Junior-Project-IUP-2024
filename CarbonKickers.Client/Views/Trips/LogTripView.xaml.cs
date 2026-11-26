using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Trips;

public partial class LogTripView : UserControl
{
    private readonly IShell _shell;
    private readonly ApiClient _apiClient;
    private IReadOnlyList<LeagueTierResponse> _tiers = [];
    private PlaceSuggestionResponse? _selectedOrigin;
    private PlaceSuggestionResponse? _selectedDestination;

    private CancellationTokenSource? _originSearchCancellation;
    private CancellationTokenSource? _destinationSearchCancellation;

    private bool _isSettingSelectedPlace;

    // The running trip-complete celebration, so it can be stopped
    private CancellationTokenSource? _celebration;

    public LogTripView(IShell shell)
    {
        _shell = shell;
        _apiClient = shell.Api;

        InitializeComponent();

        Loaded += LogTripView_Loaded;
        Unloaded += (_, _) => _celebration?.Cancel();
    }

    private async void LogTripView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var modesTask = _apiClient.GetTransportModesAsync();
            var tiersTask = _apiClient.GetLeagueTiersAsync();

            await Task.WhenAll(modesTask, tiersTask);

            var modes = await modesTask;
            _tiers = await tiersTask;

            TransportModeComboBox.ItemsSource = modes;
            BaselineModeComboBox.ItemsSource = modes;

            TransportModeComboBox.SelectedItem = modes.FirstOrDefault(mode =>
                mode.Name == "Bicycle"
            );

            BaselineModeComboBox.SelectedItem = modes.FirstOrDefault(mode => mode.Name == "Car");
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot load transport modes. Start the API first.");
            SaveTripButton.IsEnabled = false;
        }
        catch (TaskCanceledException)
        {
            ShowError("Loading transport modes timed out. Open this screen again to retry.");
            SaveTripButton.IsEnabled = false;
        }
    }

    private async void OriginTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSettingSelectedPlace)
        {
            return;
        }

        _selectedOrigin = null;

        await SearchOriginAsync(OriginTextBox.Text);
    }

    private async void DestinationTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSettingSelectedPlace)
        {
            return;
        }

        _selectedDestination = null;

        await SearchDestinationAsync(DestinationTextBox.Text);
    }

    private async Task SearchOriginAsync(string query)
    {
        _originSearchCancellation?.Cancel();
        _originSearchCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(400, _originSearchCancellation.Token);

            if (query.Trim().Length < 3)
            {
                OriginSuggestionsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var places = await _apiClient.SearchPlacesAsync(query);

            OriginSuggestionsListBox.ItemsSource = places;
            OriginSuggestionsListBox.Visibility =
                places.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (TaskCanceledException)
        {
            // Expected when the user keeps typing.
        }
        catch (HttpRequestException)
        {
            ShowError("Unable to search for origin locations.");
        }
    }

    private async Task SearchDestinationAsync(string query)
    {
        _destinationSearchCancellation?.Cancel();
        _destinationSearchCancellation = new CancellationTokenSource();

        try
        {
            await Task.Delay(400, _destinationSearchCancellation.Token);

            if (query.Trim().Length < 3)
            {
                DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
                return;
            }

            var places = await _apiClient.SearchPlacesAsync(query);

            DestinationSuggestionsListBox.ItemsSource = places;
            DestinationSuggestionsListBox.Visibility =
                places.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        catch (TaskCanceledException)
        {
            // Expected when the user keeps typing.
        }
        catch (HttpRequestException)
        {
            ShowError("Unable to search for destination locations.");
        }
    }

    private void OriginSuggestionsListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (OriginSuggestionsListBox.SelectedItem is not PlaceSuggestionResponse place)
        {
            return;
        }

        _selectedOrigin = place;

        _isSettingSelectedPlace = true;
        OriginTextBox.Text = place.FormattedAddress;
        _isSettingSelectedPlace = false;

        OriginSuggestionsListBox.Visibility = Visibility.Collapsed;
    }

    private void DestinationSuggestionsListBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (DestinationSuggestionsListBox.SelectedItem is not PlaceSuggestionResponse place)
        {
            return;
        }

        _selectedDestination = place;

        _isSettingSelectedPlace = true;
        DestinationTextBox.Text = place.FormattedAddress;
        _isSettingSelectedPlace = false;

        DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
    }

    private async void SaveTripButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Visibility = Visibility.Collapsed;

        if (_selectedOrigin is null || _selectedDestination is null)
        {
            ShowError("Select an origin and destination from the search suggestions.");
            return;
        }

        if (TransportModeComboBox.SelectedItem is not TransportModeResponse transportMode)
        {
            ShowError("Select the transport mode you used.");
            return;
        }

        if (BaselineModeComboBox.SelectedItem is not TransportModeResponse baselineMode)
        {
            ShowError("Select a comparison transport mode.");
            return;
        }

        SaveTripButton.IsEnabled = false;
        SaveTripButton.Content = "SAVING...";

        try
        {
            var request = new CreateTripRequest
            {
                Origin = _selectedOrigin.FormattedAddress,
                OriginPlaceId = _selectedOrigin.PlaceId,
                Destination = _selectedDestination.FormattedAddress,
                DestinationPlaceId = _selectedDestination.PlaceId,
                TransportModeId = transportMode.Id,
                BaselineTransportModeId = baselineMode.Id,
            };

            var result = await _apiClient.CreateTripAsync(request);

            DistanceResultText.Text =
                $"Your route ({FormatRouteMode(result.ActualRouteMode)}): "
                + $"{result.DistanceKm:F2} km";

            BaselineDistanceResultText.Text =
                $"Comparison route ({FormatRouteMode(result.BaselineRouteMode)}): "
                + $"{result.BaselineDistanceKm:F2} km";

            EmittedResultText.Text = $"Your CO₂ emitted: {result.Co2EmittedKg:F2} kg";

            BaselineEmittedResultText.Text = $"Comparison CO₂: {result.BaselineCo2EmittedKg:F2} kg";

            RouteSummaryText.Text =
                $"By {result.TransportModeName.ToLowerInvariant()} instead of "
                + $"{result.BaselineTransportModeName.ToLowerInvariant()} · {result.DistanceKm:F2} km";

            var estimates = new List<string>();

            if (result.ActualDistanceIsEstimated)
            {
                estimates.Add(
                    $"Your route was estimated using a "
                        + $"{FormatRouteMode(result.ActualRouteMode)} route."
                );
            }

            if (result.BaselineDistanceIsEstimated)
            {
                estimates.Add(
                    $"The comparison route was estimated using a "
                        + $"{FormatRouteMode(result.BaselineRouteMode)} route."
                );
            }

            RouteEstimateText.Text = string.Join(" ", estimates);
            RouteEstimateText.Visibility =
                estimates.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            await PlayTripCompleteAsync(result);
        }
        catch (InvalidOperationException exception)
        {
            ShowError(exception.Message);
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Verify that CarbonKickers.Api is running.");
        }
        catch (TaskCanceledException)
        {
            // Unlike the search boxes, nothing cancels a save on purpose, so this is the 30 s timeout
            ShowError("Saving took too long. Check your trip history before trying again, it may have been saved.");
        }
        finally
        {
            SaveTripButton.IsEnabled = true;
            SaveTripButton.Content = "SAVE TRIP";
        }
    }

    // The payout: the form makes way for the result, the numbers count up, the tier bar fills,
    // then any new badge pops in. About two seconds; instant when Windows animations are off.
    private async Task PlayTripCompleteAsync(CreateTripResponse result)
    {
        _celebration?.Cancel();
        _celebration = new CancellationTokenSource();
        var token = _celebration.Token;

        var animate = SystemParameters.ClientAreaAnimation;
        var easeOut = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 };
        var hasBadges = result.NewlyUnlockedBadges.Count > 0;

        StopCelebrationAnimations();
        PageHeader.Visibility = Visibility.Collapsed;
        FormCard.Visibility = Visibility.Collapsed;
        TierBar.Visibility = Visibility.Visible;
        TierProgressText.Visibility = Visibility.Visible;

        // Badges take their space now (invisible) so nothing below jumps when they pop in
        NewBadgesItems.ItemsSource = hasBadges ? result.NewlyUnlockedBadges : null;
        NewBadgesItems.Visibility = hasBadges ? Visibility.Visible : Visibility.Collapsed;
        NewBadgesItems.Opacity = animate ? 0 : 1;

        // Start the bar from where the player stood before this trip, so it's never empty
        var before = (_shell.Player?.GreenXp ?? 0);
        ShowTierBar(before, before);

        ResultBorder.Visibility = Visibility.Visible;
        PageScroll.ScrollToTop();

        // Ask for the new XP total while the numbers are counting
        var playerTask = _shell.RefreshPlayerAsync();

        try
        {
            if (animate)
            {
                ResultBorder.BeginAnimation(
                    OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200))
                );
                var pop = new DoubleAnimation(0.96, 1, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = easeOut,
                };
                ResultScale.BeginAnimation(ScaleTransform.ScaleXProperty, pop);
                ResultScale.BeginAnimation(ScaleTransform.ScaleYProperty, pop);

                // Arcade-style counter: a dozen chunky steps
                const int steps = 12;
                for (var i = 1; i <= steps; i++)
                {
                    var t = (double)i / steps;
                    XpResultText.Text = $"+{Math.Round(result.XpEarned * t):N0}";
                    SavedResultText.Text = $"{result.Co2SavedKg * t:F2}";
                    await Task.Delay(55, token);
                }
            }

            XpResultText.Text = $"+{result.XpEarned:N0}";
            SavedResultText.Text = $"{result.Co2SavedKg:F2}";

            var player = await playerTask;
            token.ThrowIfCancellationRequested();

            if (player is null)
            {
                TierBar.Visibility = Visibility.Collapsed;
                TierProgressText.Visibility = Visibility.Collapsed;
            }
            else
            {
                await FillTierBarAsync(
                    player.GreenXp - result.XpEarned,
                    player.GreenXp,
                    animate,
                    token
                );
            }

            if (hasBadges && animate)
            {
                // The big moment: the badge card overshoots and settles in its reserved space
                var scale = new ScaleTransform(1, 1);
                NewBadgesItems.RenderTransformOrigin = new Point(0.5, 0.5);
                NewBadgesItems.RenderTransform = scale;

                var badgePop = new DoubleAnimationUsingKeyFrames
                {
                    Duration = TimeSpan.FromMilliseconds(380),
                };
                badgePop.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.6, KeyTime.FromPercent(0)));
                badgePop.KeyFrames.Add(
                    new EasingDoubleKeyFrame(1.08, KeyTime.FromPercent(0.6), easeOut)
                );
                badgePop.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1)));
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, badgePop);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, badgePop);
                NewBadgesItems.BeginAnimation(
                    OpacityProperty,
                    new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150))
                );
                await Task.Delay(380, token);
            }

            LogAnotherButton.Focus();
        }
        catch (OperationCanceledException)
        {
            // The player moved on (logged another trip or left the screen)
        }
    }

    // Banked XP in gold; XP from this trip shows in peach underneath until the gold catches up
    private void ShowTierBar(int banked, int total)
    {
        var tier = TierForXp(total);
        var next = NextTier(tier);

        TierGain.Value = Percent(tier, next, total);
        TierProgress.Value = Percent(tier, next, banked);

        TierProgressText.Text = next is null
            ? $"{total:N0} XP · top tier reached"
            : $"{total:N0} XP · {next.MinXp - total:N0} XP to {next.TierName}";
    }

    private async Task FillTierBarAsync(
        int before,
        int after,
        bool animate,
        CancellationToken token
    )
    {
        var easeOut = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 };
        var tierBefore = TierForXp(before);
        var tierAfter = TierForXp(after);

        if (!animate)
        {
            ShowTierBar(after, after);
            return;
        }

        if (tierBefore != tierAfter)
        {
            // Tier up: fill the old tier to the brim, then start the new one from empty
            TierGain.Value = 100;
            TierProgress.BeginAnimation(
                RangeBase.ValueProperty,
                new DoubleAnimation(100, TimeSpan.FromMilliseconds(300))
                {
                    EasingFunction = easeOut,
                }
            );
            await Task.Delay(400, token);
            TierProgress.BeginAnimation(RangeBase.ValueProperty, null);
            ShowTierBar(TierForXp(after).MinXp, after);
        }
        else
        {
            ShowTierBar(before, after);
        }

        TierProgress.BeginAnimation(
            RangeBase.ValueProperty,
            new DoubleAnimation(TierGain.Value, TimeSpan.FromMilliseconds(600))
            {
                EasingFunction = easeOut,
            }
        );
        await Task.Delay(550, token);
    }

    private LeagueTierResponse TierForXp(int xp)
    {
        return _tiers.Last(tier => xp >= tier.MinXp);
    }

    private LeagueTierResponse? NextTier(LeagueTierResponse tier)
    {
        return _tiers.SkipWhile(item => item.TierName != tier.TierName).Skip(1).FirstOrDefault();
    }

    private static double Percent(LeagueTierResponse tier, LeagueTierResponse? next, int xp)
    {
        return next is null
            ? 100
            : Math.Clamp(100.0 * (xp - tier.MinXp) / (next.MinXp - tier.MinXp), 0, 100);
    }

    private void StopCelebrationAnimations()
    {
        TierProgress.BeginAnimation(RangeBase.ValueProperty, null);
        ResultBorder.BeginAnimation(OpacityProperty, null);
        NewBadgesItems.BeginAnimation(OpacityProperty, null);
        ResultScale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        ResultScale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
    }

    // Back to an empty form; the transport choices stay as they were
    private void LogAnotherButton_Click(object sender, RoutedEventArgs e)
    {
        _celebration?.Cancel();
        StopCelebrationAnimations();

        _isSettingSelectedPlace = true;
        OriginTextBox.Clear();
        DestinationTextBox.Clear();
        _isSettingSelectedPlace = false;

        _selectedOrigin = null;
        _selectedDestination = null;
        OriginSuggestionsListBox.Visibility = Visibility.Collapsed;
        DestinationSuggestionsListBox.Visibility = Visibility.Collapsed;
        ErrorText.Visibility = Visibility.Collapsed;

        ResultBorder.Visibility = Visibility.Collapsed;
        PageHeader.Visibility = Visibility.Visible;
        FormCard.Visibility = Visibility.Visible;
        OriginTextBox.Focus();
    }

    private void BackToDashboardButton_Click(object sender, RoutedEventArgs e) =>
        _shell.Navigate(Screen.Dashboard);

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private static string FormatRouteMode(string routeMode)
    {
        return routeMode switch
        {
            "DRIVE" => "driving",
            "WALK" => "walking",
            "BICYCLE" => "bicycle",
            "TWO_WHEELER" => "motorcycle",
            "TRANSIT" => "public-transit",
            _ => routeMode.Replace("_", " ").ToLowerInvariant(),
        };
    }
}
