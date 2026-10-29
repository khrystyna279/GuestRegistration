using GuestRegistration.ConsoleApp.Commands;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Menu;

internal sealed class EntityMenu<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key { get; }
    public string Description { get; }
    private readonly ConsoleMenu _operations;

    public EntityMenu(string key, string name, IRepository<TDto> repository, Func<TDto> read, Action<TDto?> print,
        string? deleteWarning = null)
    {
        Key = key;
        Description = name;
        _operations = new ConsoleMenu(name + " / CRUD", new IConsoleCommand[]
        {
            new CreateCommand<TDto>(repository, read, print),
            new ReadAllCommand<TDto>(repository, print),
            new ReadByIdCommand<TDto>(repository, print),
            new UpdateCommand<TDto>(repository, read, print),
            new DeleteCommand<TDto>(repository, deleteWarning)
        });
    }

    public void Execute() => _operations.RunOnce();
}
