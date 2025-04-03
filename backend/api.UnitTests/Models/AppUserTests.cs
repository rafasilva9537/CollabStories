using api.Models;
using NSubstitute;
using Xunit;

namespace api.UnitTests.Models;

public class AppUserTests
{
    [Fact]
    public void AppUser_CreatingEmptyUserModel_ReturnsExpectedDefaultValues()
    {
        AppUser actualUser = new AppUser();

        Assert.Equal(0, actualUser.Id);
        Assert.Equal(string.Empty, actualUser.Nickname);
        Assert.Equal(string.Empty, actualUser.Email);
        Assert.Equal(string.Empty, actualUser.ProfileImage);
        Assert.Equal(string.Empty, actualUser.Description);
        Assert.Equal(string.Empty, actualUser.ProfileImage);
        Assert.NotEqual(DateTimeOffset.MinValue, actualUser.CreatedDate);
        Assert.IsType<DateTimeOffset>(actualUser.CreatedDate);
    }

}