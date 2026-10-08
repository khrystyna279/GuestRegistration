using System;
using System.Collections.Generic;
using GuestRegistration.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BankCard> BankCards { get; set; }

    public virtual DbSet<LoginAttempt> LoginAttempts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankCard>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_BankCards_UserId");

            entity.Property(e => e.Brand)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CardHolder).HasMaxLength(100);
            entity.Property(e => e.LastFourDigits)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.User).WithMany(p => p.BankCards).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<LoginAttempt>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_LoginAttempts_UserId");

            entity.Property(e => e.AttemptedAtUtc).HasPrecision(0);
            entity.Property(e => e.LoginEntered).HasMaxLength(30);

            entity.HasOne(d => d.User).WithMany(p => p.LoginAttempts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Users_Email").IsUnique();

            entity.HasIndex(e => e.Login, "IX_Users_Login").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.CreatedAtUtc).HasPrecision(0);
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .UseCollation("Latin1_General_100_BIN2");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Gender)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Login)
                .HasMaxLength(30)
                .UseCollation("Latin1_General_100_BIN2");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RecoveryWordHash)
                .HasMaxLength(200)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
