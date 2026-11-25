using System.Net.Http;
using System.Windows;
using CarbonKickers.Client.Services;

namespace CarbonKickers.Client.Views.Badges;

public partial class MyBadgesWindow : Window
{
    private readonly ApiClient _apiClient;

    public MyBadgesWindow(ApiClient apiClient)
    {
        _apiClient = apiClient;

        InitializeComponent();

        Loaded += MyBadgesWindow_Loaded;
    }

    private async void MyBadgesWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var badges = await _apiClient.GetBadgesAsync();

            BadgesItems.ItemsSource = badges;
            EmptyText.Visibility = badges.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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
}
