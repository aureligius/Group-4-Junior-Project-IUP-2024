using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Views.Shell;

namespace CarbonKickers.Client.Views.Trips;

public partial class TripHistoryView : UserControl
{
    private readonly IShell _shell;

    public TripHistoryView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += TripHistoryView_Loaded;
    }

    private async void TripHistoryView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            var trips = await _shell.Api.GetTripsAsync();

            TripsDataGrid.ItemsSource = trips;

            if (trips.Count == 0)
            {
                TripsDataGrid.Visibility = Visibility.Collapsed;
                EmptyText.Visibility = Visibility.Visible;
                return;
            }

            SummaryText.Text =
                $"{trips.Count} trips · {trips.Sum(t => t.DistanceKm):F1} km · "
                + $"{trips.Sum(t => t.Co2SavedKg):F2} kg CO₂ kicked · {trips.Sum(t => t.XpEarned):N0} XP earned";
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

    private void LogTripButton_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.LogTrip);
}
