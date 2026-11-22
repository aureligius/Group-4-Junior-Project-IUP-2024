using System.Net.Http;
using System.Windows;
using CarbonKickers.Client.Services;

namespace CarbonKickers.Client.Views.Trips;

public partial class TripHistoryWindow : Window
{
    private readonly ApiClient _apiClient;

    public TripHistoryWindow(ApiClient apiClient)
    {
        _apiClient = apiClient;

        InitializeComponent();

        Loaded += TripHistoryWindow_Loaded;
    }

    private async void TripHistoryWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            var trips = await _apiClient.GetTripsAsync();

            TripsDataGrid.ItemsSource = trips;
            EmptyText.Visibility = trips.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowError("Unable to load trip history. Please log in again.");
        }
    }

    private void ShowError(string message)
    {
        TripsDataGrid.Visibility = Visibility.Collapsed;
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}