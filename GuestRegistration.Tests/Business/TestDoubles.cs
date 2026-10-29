using System.Text.Json;
using GuestRegistration.Business.Interfaces;
using GuestRegistration.Business.Models;
using GuestRegistration.Business.Services;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.Tests.Business;

internal sealed class FixedClock : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
}

internal sealed class TestSecretHasher : ISecretHasher
{
    public string Hash(string value) => "test-hash:" + value;
    public bool Verify(string value, string storedHash) => storedHash == Hash(value);
}

internal sealed class MemoryState
{
    public List<UserDto> Users { get; set; } = new();
    public List<BankCardDto> Cards { get; set; } = new();
    public List<LoginAttemptDto> Attempts { get; set; } = new();
    public MemoryState Copy() => Clone(this);
    public static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
}

internal sealed class MemorySessions : IDataSessionFactory
{
    public MemoryState State { get; set; } = new();
    public bool FailCardCreate { get; set; }
    public int Opened { get; private set; }
    public int Disposed { get; private set; }
    public int Committed { get; private set; }
    public bool LastTransactional { get; private set; }

    public IDataSession Open(bool transactional = false)
    {
        Opened++; LastTransactional = transactional;
        return new Session(this);
    }

    public UserDto SeedUser(string login = "guest_user", string role = "Guest")
    {
        var user = new UserDto
        {
            Id = State.Users.Count + 1, Login = login, FirstName = "Test", LastName = "User", RoleCode = role,
            PasswordHash = new TestSecretHasher().Hash("Student123!"), RecoveryWordHash = new TestSecretHasher().Hash("school"),
            Email = login + "@example.com", Phone = "+380990000001", Address = "Lviv", Gender = "Other",
            CreatedAtUtc = new FixedClock().GetUtcNow().UtcDateTime
        };
        State.Users.Add(user); return user;
    }

    private sealed class Session : IDataSession
    {
        private readonly MemorySessions _owner;
        private readonly MemoryState _copy;
        public IUserRepository Users { get; }
        public IBankCardRepository Cards { get; }
        public ILoginAttemptRepository Attempts { get; }
        public Session(MemorySessions owner)
        {
            _owner = owner; _copy = owner.State.Copy();
            Users = new MemoryUsers(_copy.Users);
            Cards = new MemoryCards(_copy.Cards, () => owner.FailCardCreate);
            Attempts = new MemoryAttempts(_copy.Attempts);
        }
        public void Commit() { _owner.State = _copy.Copy(); _owner.Committed++; }
        public void Dispose() => _owner.Disposed++;
    }
}

internal class MemoryRepository<TDto> : IRepository<TDto> where TDto : EntityDto
{
    protected readonly List<TDto> Items;
    public MemoryRepository(List<TDto> items) => Items = items;
    public virtual TDto Create(TDto dto)
    {
        var saved = MemoryState.Clone(dto);
        saved.Id = Items.Count == 0 ? 1 : Items.Max(item => item.Id) + 1;
        Items.Add(saved); return MemoryState.Clone(saved);
    }
    public List<TDto> GetAll() => Items.Select(MemoryState.Clone).ToList();
    public TDto? GetById(int id) => Items.FirstOrDefault(item => item.Id == id) is { } found ? MemoryState.Clone(found) : null;
    public bool Update(TDto dto)
    {
        int index = Items.FindIndex(item => item.Id == dto.Id);
        if (index < 0) return false;
        Items[index] = MemoryState.Clone(dto); return true;
    }
    public bool Delete(int id) => Items.RemoveAll(item => item.Id == id) != 0;
}

internal sealed class MemoryUsers : MemoryRepository<UserDto>, IUserRepository
{
    public MemoryUsers(List<UserDto> items) : base(items) { }
    public UserDto? FindByLogin(string login) => GetAll().FirstOrDefault(user => user.Login == login);
    public UserDto? FindByEmail(string email) => GetAll().FirstOrDefault(user => user.Email == email);
}
internal sealed class MemoryCards : MemoryRepository<BankCardDto>, IBankCardRepository
{
    private readonly Func<bool> _fail;
    public MemoryCards(List<BankCardDto> items, Func<bool> fail) : base(items) => _fail = fail;
    public override BankCardDto Create(BankCardDto dto) => _fail() ? throw new DbUpdateException("Test failure") : base.Create(dto);
}
internal sealed class MemoryAttempts : MemoryRepository<LoginAttemptDto>, ILoginAttemptRepository
{
    public MemoryAttempts(List<LoginAttemptDto> items) : base(items) { }
}

internal static class BusinessInputs
{
    public static RegistrationInput Registration() => new()
    {
        Login = "new_guest", FirstName = "Test", LastName = "Student", Password = "Student123!", ConfirmPassword = "Student123!",
        RecoveryWord = "school", Gender = "Other", Address = "Lviv, Test 1", Email = "new_guest@example.com",
        Phone = "+380990000001", CardNumber = "4111111111111111", CardHolder = "TEST USER", ExpirationMonth = "12", ExpirationYear = "2099"
    };
}
