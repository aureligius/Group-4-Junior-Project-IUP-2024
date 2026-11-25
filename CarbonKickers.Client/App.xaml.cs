using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;
using CarbonKickers.Client.Services;

namespace CarbonKickers.Client;

public partial class App : System.Windows.Application
{
    public App()
    {
        // Format numbers and dates the English way (1,240 XP, 18.62 kg) whatever the computer's region is
        var english = CultureInfo.GetCultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = english;
        CultureInfo.DefaultThreadCurrentUICulture = english;
        CultureInfo.CurrentCulture = english;
        CultureInfo.CurrentUICulture = english;

        // Safety net: an error no screen expected shows a message instead of closing the app.
        // Button and Loaded handlers are async void, so their errors would otherwise end up here
        // and crash the whole program.
        DispatcherUnhandledException += App_DispatcherUnhandledException;
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        // The main window is already on its way back to the login screen
        if (e.Exception is SessionExpiredException)
        {
            return;
        }

        Debug.WriteLine($"Unhandled error: {e.Exception}");

        var message = e.Exception switch
        {
            HttpRequestException => "Cannot reach the API. Check that CarbonKickers.Api is running.",
            TaskCanceledException => "The API took too long to answer. Please try again.",
            _ => $"Something went wrong: {e.Exception.Message}",
        };

        MessageBox.Show(
            message + "\n\nThe app is still running; you can carry on.",
            "Carbon Kickers",
            MessageBoxButton.OK,
            MessageBoxImage.Warning
        );
    }
}
