namespace CarbonKickers.Domain.Entities;

using CarbonKickers.Domain.Enums;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int GreenXp { get; private set; } = 0;
    public double TotalCo2SavedKg { get; private set; } = 0.0;
    public UserRole Role { get; set; } = UserRole.Member;

    public void Register() 
    { 
        // Perlu Registrasi Logic Nanti
    }

    public bool Login() 
    { 
        return true; //Buat sekarang semuanya bisa login
    }

    public void AddXp(int points)
    {
        if (points > 0)
        {
            GreenXp += points;
            UpdateTier();
        }
    }

    public void AddCo2Saved(double savedKg)
    {
        if (savedKg > 0)
        {
            TotalCo2SavedKg += savedKg;
        }
    }

    public void UpdateTier() 
    { 
        // Tambahin Logic
    }
}