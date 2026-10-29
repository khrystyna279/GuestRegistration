using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.DAL.Interfaces;

namespace GuestRegistration.Business.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IDataSessionFactory _data;
    private readonly UserSession _session;
    public ProfileService(IDataSessionFactory data, UserSession session) { _data = data; _session = session; }

    public UserProfile GetCurrentProfile()
    {
        var current = _session.Current ?? throw new UnauthorizedAccessException("Спочатку увійдіть.");
        using var data = _data.Open();
        var user = data.Users.GetById(current.Id);
        // Recheck the persisted role rather than trusting the view or a cached UI flag.
        if (user == null || user.RoleCode != "Guest") throw new UnauthorizedAccessException("Для цього розділу потрібна роль Guest.");
        var cards = data.Cards.GetAll().Where(card => card.UserId == user.Id)
            .Select(card => new CardSummary(card.CardHolder, card.Brand, card.LastFourDigits, card.ExpirationMonth, card.ExpirationYear)).ToList();
        return new UserProfile(user.Login, user.FirstName, user.LastName, user.Gender, user.Address, user.Email, user.Phone, cards);
    }
}
