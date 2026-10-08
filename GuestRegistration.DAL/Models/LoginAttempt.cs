using System;
using System.Collections.Generic;

namespace GuestRegistration.DAL.Models;

public partial class LoginAttempt
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string LoginEntered { get; set; } = null!;

    public DateTime AttemptedAtUtc { get; set; }

    public bool IsSuccessful { get; set; }

    public virtual User? User { get; set; }
}
