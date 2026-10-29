using GuestRegistration.Business.Services;
using Xunit;

namespace GuestRegistration.Tests.Business;

public sealed class SecretHasherUnitTests
{
    private readonly Pbkdf2SecretHasher _hasher = new();

    [Fact]
    public void HashUsesSaltAndVerifiesOnlyCorrectValue()
    {
        string first = _hasher.Hash("Student123!"); string second = _hasher.Hash("Student123!");
        Assert.NotEqual(first, second);
        Assert.True(first.Length <= 200);
        Assert.True(_hasher.Verify("Student123!", first));
        Assert.False(_hasher.Verify("wrong", first));
    }

    [Fact]
    public void InitialSeedHashesRemainReadable() => Assert.True(_hasher.Verify("school", "EDmw3NXWWozqRlQkPkvEyg==.vRQ5gSWb3p+o5lqUgKW9KFKDvSsfYrq8ALiqO+rvs8k="));

    [Theory]
    [InlineData("")]
    [InlineData("demo_hash")]
    [InlineData("not.a.hash")]
    [InlineData("YQ==.Yg==")]
    [InlineData("pbkdf2-sha256$999999999$AA$AA")]
    [InlineData("pbkdf2-sha256$bad$AA$AA")]
    public void MalformedHashesReturnFalse(string hash) => Assert.False(_hasher.Verify("password", hash));
}
