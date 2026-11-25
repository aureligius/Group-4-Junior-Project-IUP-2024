using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using CarbonKickers.Client.Views.Shell;
using CarbonKickers.Shared.Contracts;

namespace CarbonKickers.Client.Views.Admin;

public partial class AdminView : UserControl
{
    private readonly IShell _shell;
    private AdminBadgeResponse? _selectedBadge;
    private AdminTransportModeResponse? _selectedTransportMode;
    private LeagueTierRuleAdminResponse? _selectedLeagueRule;

    public AdminView(IShell shell)
    {
        _shell = shell;

        InitializeComponent();

        Loaded += AdminView_Loaded;
    }

    private async void AdminView_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadSummaryAsync();
        await LoadBadgesAsync();
        await LoadTransportModesAsync();
        await LoadLeagueRulesAsync();
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            var summary = await _shell.Api.GetAdminSummaryAsync();

            TotalUsersText.Text = summary.TotalUsers.ToString("N0");
            TotalTripsText.Text = summary.TotalTrips.ToString("N0");
            TotalCo2Text.Text = $"{summary.TotalCo2SavedKg:F1} kg";
            BadgesUnlockedText.Text = summary.TotalBadgesUnlocked.ToString("N0");
            ActiveChallengesText.Text = summary.ActiveChallenges.ToString("N0");
            ActiveLeaguesText.Text = summary.ActiveLeagues.ToString("N0");

            ErrorText.Visibility = Visibility.Collapsed;
        }
        catch (HttpRequestException)
        {
            ShowError("Cannot reach the API. Start CarbonKickers.Api first.");
        }
        catch
        {
            ShowError("You are not authorized to view the admin panel.");
        }
    }

    private async void CreateWeeklyLeaguesButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = await _shell.Api.CreateCurrentWeekLeaguesAsync();

            LeagueStatusText.Text =
                $"Ready: {result.Leagues.Count} leagues exist from "
                + $"{result.WeekStartUtc.ToLocalTime():d MMM} to "
                + $"{result.WeekEndUtc.ToLocalTime():d MMM}.";

            await LoadSummaryAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async Task LoadBadgesAsync()
    {
        try
        {
            BadgesListBox.ItemsSource = await _shell.Api.GetAdminBadgesAsync();
        }
        catch (Exception)
        {
            ShowError("Unable to load badges.");
        }
    }

    private void BadgesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (BadgesListBox.SelectedItem is not AdminBadgeResponse badge)
        {
            return;
        }

        _selectedBadge = badge;

        BadgeTitleTextBox.Text = badge.Title;
        BadgeDescriptionTextBox.Text = badge.Description;
        BadgeXpTextBox.Text = badge.XpRequirement.ToString();
        BadgeIconUrlTextBox.Text = badge.IconUrl;

        BadgeStatusText.Text = $"Editing “{badge.Title}”.";
    }

    private async void CreateBadgeButton_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildBadgeRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var badge = await _shell.Api.CreateAdminBadgeAsync(request);

            BadgeStatusText.Text = $"Created “{badge.Title}”.";
            ClearBadgeForm();
            await LoadBadgesAsync();
            await LoadSummaryAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async void UpdateBadgeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedBadge is null)
        {
            BadgeStatusText.Text = "Select a badge from the list first.";
            return;
        }

        var request = BuildBadgeRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var badge = await _shell.Api.UpdateAdminBadgeAsync(_selectedBadge.Id, request);

            BadgeStatusText.Text = $"Updated “{badge.Title}”.";
            await LoadBadgesAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ClearBadgeButton_Click(object sender, RoutedEventArgs e)
    {
        ClearBadgeForm();
    }

    private SaveBadgeRequest? BuildBadgeRequest()
    {
        if (
            string.IsNullOrWhiteSpace(BadgeTitleTextBox.Text)
            || string.IsNullOrWhiteSpace(BadgeDescriptionTextBox.Text)
            || string.IsNullOrWhiteSpace(BadgeIconUrlTextBox.Text)
        )
        {
            BadgeStatusText.Text = "Title, description, and icon URL are required.";
            return null;
        }

        if (!int.TryParse(BadgeXpTextBox.Text, out var xpRequirement) || xpRequirement < 0)
        {
            BadgeStatusText.Text = "Required Green XP must be a whole number of zero or more.";
            return null;
        }

        return new SaveBadgeRequest
        {
            Title = BadgeTitleTextBox.Text.Trim(),
            Description = BadgeDescriptionTextBox.Text.Trim(),
            XpRequirement = xpRequirement,
            IconUrl = BadgeIconUrlTextBox.Text.Trim(),
        };
    }

    private void ClearBadgeForm()
    {
        _selectedBadge = null;
        BadgesListBox.SelectedItem = null;

        BadgeTitleTextBox.Clear();
        BadgeDescriptionTextBox.Clear();
        BadgeXpTextBox.Clear();
        BadgeIconUrlTextBox.Clear();

        BadgeStatusText.Text = "Ready to create a new badge.";
    }

    private async Task LoadTransportModesAsync()
    {
        try
        {
            TransportModesListBox.ItemsSource = await _shell.Api.GetAdminTransportModesAsync();
        }
        catch (Exception)
        {
            ShowError("Unable to load transport modes.");
        }
    }

    private void TransportModesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TransportModesListBox.SelectedItem is not AdminTransportModeResponse mode)
        {
            return;
        }

        _selectedTransportMode = mode;

        ModeNameTextBox.Text = mode.Name;
        EmissionFactorTextBox.Text = mode.EmissionFactorPerKm.ToString();
        ModeIsActiveCheckBox.IsChecked = mode.IsActive;

        ModeStatusText.Text = $"Editing “{mode.Name}”.";
    }

    private async void CreateModeButton_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildTransportModeRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var mode = await _shell.Api.CreateAdminTransportModeAsync(request);

            ModeStatusText.Text = $"Created “{mode.Name}”.";
            ClearModeForm();

            await LoadTransportModesAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async void UpdateModeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedTransportMode is null)
        {
            ModeStatusText.Text = "Select a transport mode from the list first.";
            return;
        }

        var request = BuildTransportModeRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var mode = await _shell.Api.UpdateAdminTransportModeAsync(
                _selectedTransportMode.Id,
                request
            );

            ModeStatusText.Text = $"Updated “{mode.Name}”.";
            await LoadTransportModesAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ClearModeButton_Click(object sender, RoutedEventArgs e)
    {
        ClearModeForm();
    }

    private SaveTransportModeRequest? BuildTransportModeRequest()
    {
        if (string.IsNullOrWhiteSpace(ModeNameTextBox.Text))
        {
            ModeStatusText.Text = "A transport-mode name is required.";
            return null;
        }

        if (
            !double.TryParse(EmissionFactorTextBox.Text, out var emissionFactor)
            || emissionFactor < 0
        )
        {
            ModeStatusText.Text = "Emission factor must be a number that is zero or greater.";
            return null;
        }

        return new SaveTransportModeRequest
        {
            Name = ModeNameTextBox.Text.Trim(),
            EmissionFactorPerKm = emissionFactor,
            IsActive = ModeIsActiveCheckBox.IsChecked == true,
        };
    }

    private void ClearModeForm()
    {
        _selectedTransportMode = null;
        TransportModesListBox.SelectedItem = null;

        ModeNameTextBox.Clear();
        EmissionFactorTextBox.Clear();
        ModeIsActiveCheckBox.IsChecked = true;

        ModeStatusText.Text = "Ready to create a new transport mode.";
    }

    private async Task LoadLeagueRulesAsync()
    {
        try
        {
            LeagueRulesListBox.ItemsSource = await _shell.Api.GetLeagueTierRulesAsync();
        }
        catch (Exception)
        {
            ShowError("Unable to load league tier rules.");
        }
    }

    private void LeagueRulesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LeagueRulesListBox.SelectedItem is not LeagueTierRuleAdminResponse rule)
        {
            return;
        }

        _selectedLeagueRule = rule;

        TierNameTextBox.Text = rule.TierName;
        TierMinXpTextBox.Text = rule.MinXp.ToString();
        TierMaxXpTextBox.Text = rule.MaxXp?.ToString() ?? "";
        TierIsActiveCheckBox.IsChecked = rule.IsActive;

        LeagueRuleStatusText.Text = $"Editing “{rule.TierName}”.";
    }

    private async void CreateLeagueRuleButton_Click(object sender, RoutedEventArgs e)
    {
        var request = BuildLeagueRuleRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var rule = await _shell.Api.CreateLeagueTierRuleAsync(request);

            LeagueRuleStatusText.Text = $"Created “{rule.TierName}”.";
            ClearLeagueRuleForm();

            await LoadLeagueRulesAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private async void UpdateLeagueRuleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLeagueRule is null)
        {
            LeagueRuleStatusText.Text = "Select a tier rule first.";
            return;
        }

        var request = BuildLeagueRuleRequest();

        if (request is null)
        {
            return;
        }

        try
        {
            var rule = await _shell.Api.UpdateLeagueTierRuleAsync(_selectedLeagueRule.Id, request);

            LeagueRuleStatusText.Text = $"Updated “{rule.TierName}”.";
            await LoadLeagueRulesAsync();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void ClearLeagueRuleButton_Click(object sender, RoutedEventArgs e)
    {
        ClearLeagueRuleForm();
    }

    private SaveLeagueTierRuleRequest? BuildLeagueRuleRequest()
    {
        if (string.IsNullOrWhiteSpace(TierNameTextBox.Text))
        {
            LeagueRuleStatusText.Text = "A tier name is required.";
            return null;
        }

        if (!int.TryParse(TierMinXpTextBox.Text, out var minXp) || minXp < 0)
        {
            LeagueRuleStatusText.Text = "Minimum XP must be a whole number of zero or more.";
            return null;
        }

        int? maxXp = null;

        if (!string.IsNullOrWhiteSpace(TierMaxXpTextBox.Text))
        {
            if (!int.TryParse(TierMaxXpTextBox.Text, out var parsedMaxXp) || parsedMaxXp < minXp)
            {
                LeagueRuleStatusText.Text = "Maximum XP must be blank or at least the minimum XP.";
                return null;
            }

            maxXp = parsedMaxXp;
        }

        return new SaveLeagueTierRuleRequest
        {
            TierName = TierNameTextBox.Text.Trim(),
            MinXp = minXp,
            MaxXp = maxXp,
            IsActive = TierIsActiveCheckBox.IsChecked == true,
        };
    }

    private void ClearLeagueRuleForm()
    {
        _selectedLeagueRule = null;
        LeagueRulesListBox.SelectedItem = null;

        TierNameTextBox.Clear();
        TierMinXpTextBox.Clear();
        TierMaxXpTextBox.Clear();
        TierIsActiveCheckBox.IsChecked = true;

        LeagueRuleStatusText.Text = "Ready to create a new tier rule.";
    }

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
