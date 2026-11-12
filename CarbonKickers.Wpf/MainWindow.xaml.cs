using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using CarbonKickers.Application.Users;

namespace CarbonKickers.Wpf;

public partial class MainWindow : Window
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7211/")
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    private void ShowRegisterButton_Click(object sender, RoutedEventArgs e)
    {
        LoginPanel.Visibility = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Visible;
    }

    private void ShowLoginButton_Click(object sender, RoutedEventArgs e)
    {
        RegisterPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        LoginErrorText.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(LoginEmailTextBox.Text) ||
            string.IsNullOrWhiteSpace(LoginPasswordBox.Password))
        {
            ShowLoginError("Email and password are required.");
            return;
        }

        try
        {
            var request = new LoginRequest(
                LoginEmailTextBox.Text.Trim(),
                LoginPasswordBox.Password);

            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/login",
                request);

            if (!response.IsSuccessStatusCode)
            {
                ShowLoginError("Invalid email or password.");
                return;
            }

            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();

            if (result is null || string.IsNullOrWhiteSpace(result.Token))
            {
                ShowLoginError("The API returned an invalid login response.");
                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token);

            MessageBox.Show(
                $"Welcome back, {result.Username}!",
                "Carbon Kickers",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Open DashboardWindow here after you create it.
        }
        catch (HttpRequestException)
        {
            ShowLoginError(
                "Cannot reach the API. Start CarbonKickers.Web first.");
        }
        catch (TaskCanceledException)
        {
            ShowLoginError("The API request timed out.");
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        RegisterErrorText.Visibility = Visibility.Collapsed;

        if (string.IsNullOrWhiteSpace(RegisterUsernameTextBox.Text) ||
            string.IsNullOrWhiteSpace(RegisterEmailTextBox.Text) ||
            string.IsNullOrWhiteSpace(RegisterPasswordBox.Password))
        {
            ShowRegisterError("All fields are required.");
            return;
        }

        try
        {
            var request = new RegisterRequest(
                RegisterUsernameTextBox.Text.Trim(),
                RegisterEmailTextBox.Text.Trim(),
                RegisterPasswordBox.Password);

            var response = await _httpClient.PostAsJsonAsync(
                "api/auth/register",
                request);

            if (!response.IsSuccessStatusCode)
            {
                ShowRegisterError(
                    "Registration failed. Email or username may already be registered.");
                return;
            }

            MessageBox.Show(
                "Account created. You can now log in.",
                "Carbon Kickers",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            RegisterUsernameTextBox.Clear();
            RegisterEmailTextBox.Clear();
            RegisterPasswordBox.Clear();

            ShowLoginButton_Click(sender, e);
        }
        catch (HttpRequestException)
        {
            ShowRegisterError(
                "Cannot reach the API. Start CarbonKickers.Web first.");
        }
        catch (TaskCanceledException)
        {
            ShowRegisterError("The API request timed out.");
        }
    }

    private void ShowLoginError(string message)
    {
        LoginErrorText.Text = message;
        LoginErrorText.Visibility = Visibility.Visible;
    }

    private void ShowRegisterError(string message)
    {
        RegisterErrorText.Text = message;
        RegisterErrorText.Visibility = Visibility.Visible;
    }
}