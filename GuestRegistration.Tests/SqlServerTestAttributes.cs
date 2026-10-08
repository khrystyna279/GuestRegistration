using Xunit;

namespace GuestRegistration.Tests;

public class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GUEST_TEST_CONNECTION")))
            Skip = "SQL Server is not configured. Set GUEST_TEST_CONNECTION to run this integration test.";
    }
}

public class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GUEST_TEST_CONNECTION")))
            Skip = "SQL Server is not configured. Set GUEST_TEST_CONNECTION to run this integration test.";
    }
}
