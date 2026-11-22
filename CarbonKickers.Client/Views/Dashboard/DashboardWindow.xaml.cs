using System.Net.Http;
using System.Windows;
using CarbonKickers.Client.Services;
using CarbonKickers.Client.Views.Trips;

namespace CarbonKickers.Client.Views.Dashboard;

public partial class DashboardWindow : Window
{
    private readonly ApiClient _apiClient;

    public DashboardWindow(ApiClient apiClient)
    {
        _apiClient = apiClient;

        InitializeComponent();

        Loaded += DashboardWindow_Loaded;
    }

    private async void DashboardWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            var dashboard = await _apiClient.GetDashboardAsync();

            WelcomeText.Text = $"Welcome back, {dashboard.Username}!";
            GreenXpText.Text = dashboard.GreenXp.ToString();
            Co2SavedText.Text = $"{dashboard.TotalCo2SavedKg:F2} kg";
            TotalTripsText.Text = dashboard.TotalTrips.ToString();

            DailyImpactItems.ItemsSource = dashboard.LastSevenDays;
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowError("Unable to load your dashboard. Please log in again.");
        }
    }

    private void ShowError(string message)
    {
        DashboardErrorText.Text = message;
        DashboardErrorText.Visibility = Visibility.Visible;
    }

    private async void LogTripButton_Click(object sender, RoutedEventArgs e)
    {
        var logTripWindow = new LogTripWindow(_apiClient) { Owner = this };

        logTripWindow.ShowDialog();

        await LoadDashboardAsync();
    }

    private void TripHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        var historyWindow = new TripHistoryWindow(_apiClient) { Owner = this };

        historyWindow.ShowDialog();
    }
}
