using System;
using System.Collections.Generic;

namespace GuestRegistration.DAL.Models;

public partial class BankCard
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string CardHolder { get; set; } = null!;

    public string Brand { get; set; } = null!;

    public string LastFourDigits { get; set; } = null!;

    public int ExpirationMonth { get; set; }

    public int ExpirationYear { get; set; }

    public virtual User User { get; set; } = null!;
}
