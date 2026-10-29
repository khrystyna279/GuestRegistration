using GuestRegistration.Business.Services;
using Xunit;

namespace GuestRegistration.Tests.Business;

public sealed class AccountValidatorUnitTests
{
    private readonly AccountValidator _validator = new(new FixedClock());

    [Fact]
    public void EmptyRegistrationReportsEveryRequiredField()
    {
        var errors = _validator.Registration(new());
        foreach (string field in new[] { "Login", "FirstName", "LastName", "Password", "ConfirmPassword", "RecoveryWord", "Gender", "Address", "Email", "Phone", "CardNumber", "CardHolder", "ExpirationMonth", "ExpirationYear" })
            Assert.Contains(field, errors.Keys);
    }

    [Theory]
    [InlineData("4111 1111 1111 1111")]
    [InlineData("4111-1111-1111-1111")]
    [InlineData("5555555555554444")]
    public void ValidCardsAreAccepted(string number) => Assert.Empty(_validator.Registration(BusinessInputs.Registration() with { CardNumber = number }));

    [Fact]
    public void ExpiredMonthIsRejected() => Assert.Contains("ExpirationMonth", _validator.Registration(BusinessInputs.Registration() with { ExpirationMonth = "9", ExpirationYear = "2026" }).Keys);

    [Theory]
    [InlineData("abcdabcdabcdabcd")]
    [InlineData("123")]
    [InlineData("000000000000000x")]
    public void InvalidCardNumbersAreRejected(string number) => Assert.Contains("CardNumber", _validator.Registration(BusinessInputs.Registration() with { CardNumber = number }).Keys);

    [Fact]
    public void WhitespaceOnlyPasswordIsRejected() => Assert.Contains("Password", _validator.Registration(BusinessInputs.Registration() with { Password = "        ", ConfirmPassword = "        " }).Keys);
}
