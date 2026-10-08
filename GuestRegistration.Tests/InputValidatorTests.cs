using GuestRegistration.DAL.Services;
using Xunit;

namespace GuestRegistration.Tests;

public class InputValidatorTests
{
    [Theory]
    [InlineData("4111111111111111", "Visa", "1111")]
    [InlineData("5555555555554444", "Mastercard", "4444")]
    [InlineData("4111 1111 1111 1111", "Visa", "1111")]
    public void CreateCard_ValidNumberReturnsOnlyMaskedData(string number, string brand, string lastFour)
    {
        var card = InputValidator.CreateCard(7, number, "TEST STUDENT", 12, 2099);
        Assert.Equal(7, card.UserId);
        Assert.Equal(brand, card.Brand);
        Assert.Equal(lastFour, card.LastFourDigits);
    }

    [Theory]
    [InlineData("4111111111111112")]
    [InlineData("123")]
    [InlineData("abcdefghijklmnop")]
    public void CreateCard_InvalidNumberIsRejected(string number) =>
        Assert.Throws<ArgumentException>(() => InputValidator.CreateCard(1, number, "TEST", 12, 2099));

    [Theory]
    [InlineData("short", "short")]
    [InlineData("Student123!", "different")]
    public void ValidatePassword_RejectsInvalidInput(string password, string confirmation) =>
        Assert.Throws<ArgumentException>(() => InputValidator.ValidatePassword(password, confirmation));

    [Fact]
    public void ValidateUser_AcceptsValidProfile() => InputValidator.ValidateUser(TestDatabase.NewUser());

    [Theory]
    [InlineData("email")]
    [InlineData("phone")]
    [InlineData("login")]
    [InlineData("name")]
    public void ValidateUser_RejectsInvalidProfile(string field)
    {
        var user = TestDatabase.NewUser();
        switch (field)
        {
            case "email": user.Email = "wrong"; break;
            case "phone": user.Phone = "wrong"; break;
            case "login": user.Login = "wrong login"; break;
            case "name": user.FirstName = " "; break;
        }
        Assert.Throws<ArgumentException>(() => InputValidator.ValidateUser(user));
    }
}
