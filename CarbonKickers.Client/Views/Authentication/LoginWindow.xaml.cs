using System.Windows;
using System.Windows.Controls;
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

    // Opens the login form with a note on top, e.g. after the session expired
    public LoginWindow(string notice)
        : this()
    {
        ShowLoginError(notice);
    }

    private void ShowRegisterButton_Click(object sender, RoutedEventArgs e) => ShowRegisterForm();

    private void ShowLoginButton_Click(object sender, RoutedEventArgs e) => ShowLoginForm();

    private void ShowRegisterForm()
    {
        LoginPanel.Visibility = Visibility.Collapsed;
        RegisterPanel.Visibility = Visibility.Visible;
        LoginSuccessText.Visibility = Visibility.Collapsed;

        // Enter submits the form that is on screen
        LoginButton.IsDefault = false;
        RegisterButton.IsDefault = true;

        RegisterUsernameTextBox.Focus();
    }

    private void ShowLoginForm()
    {
        RegisterPanel.Visibility = Visibility.Collapsed;
        LoginPanel.Visibility = Visibility.Visible;

        RegisterButton.IsDefault = false;
        LoginButton.IsDefault = true;

        LoginEmailTextBox.Focus();
    }

    private async void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        LoginErrorText.Visibility = Visibility.Collapsed;
        LoginSuccessText.Visibility = Visibility.Collapsed;

        if (
            string.IsNullOrWhiteSpace(LoginEmailTextBox.Text)
            || string.IsNullOrWhiteSpace(LoginPasswordBox.Password)
        )
        {
            ShowLoginError("Email and password are required.");
            return;
        }

        SetBusy(LoginButton, "LOGGING IN...");

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
        catch (InvalidOperationException exception)
        {
            // A server-side failure (not a wrong password); LoginAsync puts the reason in the message
            ShowLoginError(exception.Message);
        }
        finally
        {
            ClearBusy(LoginButton, "LOG IN");
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

        SetBusy(RegisterButton, "CREATING ACCOUNT...");

        try
        {
            var request = new RegisterRequest(
                RegisterUsernameTextBox.Text.Trim(),
                RegisterEmailTextBox.Text.Trim(),
                RegisterPasswordBox.Password
            );

            var error = await _apiClient.RegisterAsync(request);

            if (error is not null)
            {
                ShowRegisterError(error);
                return;
            }

            // Hand the new player straight to the login form with their email filled in
            var email = request.Email;

            RegisterUsernameTextBox.Clear();
            RegisterEmailTextBox.Clear();
            RegisterPasswordBox.Clear();

            ShowLoginForm();

            LoginErrorText.Visibility = Visibility.Collapsed;
            LoginEmailTextBox.Text = email;
            LoginPasswordBox.Clear();
            LoginSuccessText.Text = "Account created. Enter your password to start playing.";
            LoginSuccessText.Visibility = Visibility.Visible;
            LoginPasswordBox.Focus();
        }
        catch (HttpRequestException)
        {
            ShowRegisterError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch (TaskCanceledException)
        {
            ShowRegisterError("The API request timed out.");
        }
        finally
        {
            ClearBusy(RegisterButton, "CREATE ACCOUNT");
        }
    }

    // Disabling the button stops double-clicks from sending the request twice
    private static void SetBusy(Button button, string busyText)
    {
        button.IsEnabled = false;
        button.Content = busyText;
    }

    private static void ClearBusy(Button button, string idleText)
    {
        button.IsEnabled = true;
        button.Content = idleText;
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
