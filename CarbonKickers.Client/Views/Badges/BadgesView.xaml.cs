using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Views.Shell;

namespace CarbonKickers.Client.Views.Badges;

public partial class BadgesView : UserControl
{
    private readonly IShell _shell;

    public BadgesView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += BadgesView_Loaded;
    }

    private async void BadgesView_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var badges = await _shell.Api.GetBadgesAsync();

            BadgesItems.ItemsSource = badges;
            EmptyText.Visibility = badges.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            SummaryText.Text = badges.Count == 0
                ? "Your unlocked Carbon Kickers achievements."
                : $"{badges.Count} unlocked so far. Keep logging greener trips to earn more.";
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (Exception)
        {
            ShowError("Unable to load your badges. Please log in again.");
        }
    }

    private void ShowError(string message)
    {
        BadgesItems.Visibility = Visibility.Collapsed;
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void LogTripButton_Click(object sender, RoutedEventArgs e) => _shell.Navigate(Screen.LogTrip);
}
