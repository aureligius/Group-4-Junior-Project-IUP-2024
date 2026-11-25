using System.Globalization;

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
    }
}
