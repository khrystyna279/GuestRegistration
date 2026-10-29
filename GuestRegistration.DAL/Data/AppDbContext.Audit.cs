using GuestRegistration.DAL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GuestRegistration.DAL.Data;

public partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        ConfigureAudit<User>(modelBuilder, "Users");
        ConfigureAudit<BankCard>(modelBuilder, "BankCards");
        ConfigureAudit<LoginAttempt>(modelBuilder, "LoginAttempts");
    }

    private static void ConfigureAudit<TEntity>(ModelBuilder builder, string tableName) where TEntity : class
    {
        var entity = builder.Entity<TEntity>();
        // SQL Server generates audit values; triggers are incompatible with a bare OUTPUT clause.
        entity.ToTable(tableName, table => table.UseSqlOutputClause(false));
        foreach (string name in new[] { "RowInsertTime", "UpdateTime" })
        {
            var property = entity.Property<DateTime>(name)
                .HasColumnType("datetime2(7)")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .ValueGeneratedOnAddOrUpdate();
            property.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            property.Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        }
    }
}
