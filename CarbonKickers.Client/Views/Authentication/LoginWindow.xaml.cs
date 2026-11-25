using System.Windows;
using System.Net.Http;
using CarbonKickers.Client.Services;
using CarbonKickers.Shared.Contracts;
using CarbonKickers.Client.Views.Shell;

namespace CarbonKickers.Client.Views.Authentication;

public partial class LoginWindow : Window
{
    private readonly ApiClient _apiClient = new();

    public LoginWindow()
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

        if (
            string.IsNullOrWhiteSpace(LoginEmailTextBox.Text)
            || string.IsNullOrWhiteSpace(LoginPasswordBox.Password)
        )
        {
            ShowLoginError("Email and password are required.");
            return;
        }

        try
        {
            var request = new LoginRequest(
                LoginEmailTextBox.Text.Trim(),
                LoginPasswordBox.Password
            );

            var result = await _apiClient.LoginAsync(request);

            if (result is null)
            {
                ShowLoginError("Invalid email or password.");
                return;
            }

            var mainWindow = new MainWindow(_apiClient);
            mainWindow.Show();

            Close();
        }
        catch (HttpRequestException)
        {
            ShowLoginError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (TaskCanceledException)
        {
            ShowLoginError("The API request timed out.");
        }
    }

    private async void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        RegisterErrorText.Visibility = Visibility.Collapsed;

        if (
            string.IsNullOrWhiteSpace(RegisterUsernameTextBox.Text)
            || string.IsNullOrWhiteSpace(RegisterEmailTextBox.Text)
            || string.IsNullOrWhiteSpace(RegisterPasswordBox.Password)
        )
        {
            ShowRegisterError("All fields are required.");
            return;
        }

        try
        {
            var request = new RegisterRequest(
                RegisterUsernameTextBox.Text.Trim(),
                RegisterEmailTextBox.Text.Trim(),
                RegisterPasswordBox.Password
            );

            var succeeded = await _apiClient.RegisterAsync(request);

            if (!succeeded)
            {
                ShowRegisterError(
                    "Registration failed. Email or username may already be registered."
                );
                return;
            }

            MessageBox.Show(
                "Account created. You can now log in.",
                "Carbon Kickers",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            RegisterUsernameTextBox.Clear();
            RegisterEmailTextBox.Clear();
            RegisterPasswordBox.Clear();

            ShowLoginButton_Click(sender, e);
        }
        catch (HttpRequestException)
        {
            ShowRegisterError("Cannot reach the API. Start CarbonKickers.Api first.");
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
