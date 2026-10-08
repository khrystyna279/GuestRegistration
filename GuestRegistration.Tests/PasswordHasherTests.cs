using GuestRegistration.DAL.Services;
using Xunit;

namespace GuestRegistration.Tests;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_UsesDifferentSaltsForTheSamePassword()
    {
        string first = PasswordHasher.Hash("Student123!");
        string second = PasswordHasher.Hash("Student123!");
        Assert.NotEqual(first, second);
        Assert.True(PasswordHasher.Verify("Student123!", first));
        Assert.False(PasswordHasher.Verify("wrong", first));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not.a.hash")]
    [InlineData("invalid.invalid")]
    [InlineData("YQ==.Yg==")]
    public void Verify_MalformedHashReturnsFalse(string hash) =>
        Assert.False(PasswordHasher.Verify("password", hash));
}
