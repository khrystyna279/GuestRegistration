using System;
using System.Collections.Generic;

namespace GuestRegistration.DAL.Models;

public partial class User
{
    public int Id { get; set; }

    public DateTime RowInsertTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public string RoleCode { get; set; } = "Guest";

    public string Login { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string RecoveryWordHash { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public virtual ICollection<BankCard> BankCards { get; set; } = new List<BankCard>();

    public virtual ICollection<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
}
