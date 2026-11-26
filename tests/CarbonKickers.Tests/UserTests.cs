using CarbonKickers.Domain.Entities;

namespace CarbonKickers.Tests;

public sealed class UserTests
{
    [Fact]
    public void AddXp_IgnoresZeroAndNegativeValues()
    {
        var user = new User();

        user.AddXp(0);
        user.AddXp(-10);

        Assert.Equal(0, user.GreenXp);
    }

    [Fact]
    public void AddXp_AddsPositivePoints()
    {
        var user = new User();

        user.AddXp(25);

        Assert.Equal(25, user.GreenXp);
    }

    [Fact]
    public void AddCo2Saved_IgnoresNegativeValues()
    {
        var user = new User();

        user.AddCo2Saved(-2);

        Assert.Equal(0, user.TotalCo2SavedKg);
    }
}