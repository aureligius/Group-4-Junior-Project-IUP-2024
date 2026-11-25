using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Services;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Trips;

public partial class LogTripWindow : Window
{
    private readonly ApiClient _apiClient;

    private PlaceSuggestionResponse? _selectedOrigin;
    private PlaceSuggestionResponse? _selectedDestination;

    private CancellationTokenSource? _originSearchCancellation;
    private CancellationTokenSource? _destinationSearchCancellation;

    private bool _isSettingSelectedPlace;

    public LogTripWindow(ApiClient apiClient)
    {
        _apiClient = apiClient;

        InitializeComponent();

        Loaded += LogTripWindow_Loaded;
    }

    private async void LogTripWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var modes = await _apiClient.GetTransportModesAsync();

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
        ResultBorder.Visibility = Visibility.Collapsed;

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
        SaveTripButton.Content = "Saving...";

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

            SavedResultText.Text = $"CO₂ saved: {result.Co2SavedKg:F2} kg";

            XpResultText.Text = $"Green XP earned: {result.XpEarned}";

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

            ResultBorder.Visibility = Visibility.Visible;

            if (result.NewlyUnlockedBadges.Count > 0)
            {
                var badgeNames = string.Join(
                    ", ",
                    result.NewlyUnlockedBadges.Select(badge => badge.Title)
                );

                MessageBox.Show(
                    $"Badge unlocked: {badgeNames}",
                    "Carbon Kickers",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
        }
        catch (InvalidOperationException exception)
        {
            ShowError(exception.Message);
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Verify that CarbonKickers.Api is running.");
        }
        finally
        {
            SaveTripButton.IsEnabled = true;
            SaveTripButton.Content = "Save Trip";
        }
    }

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
