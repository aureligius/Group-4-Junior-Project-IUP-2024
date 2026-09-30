using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CarbonKickers.Web.Pages;

public class IndexModel : PageModel
{
    // Variables defined here can be accessed directly in your HTML
    public string Username { get; set; } = string.Empty;
    public double TotalCo2Saved { get; set; }

    // OnGet() runs automatically when a user visits the /dashboard URL
    public void OnGet()
    {
        // In the future, you will query your database here
        Username = "EcoWarrior99";
        TotalCo2Saved = 42.5;
    }
}