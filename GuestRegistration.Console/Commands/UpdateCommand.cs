using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Commands;

internal sealed class UpdateCommand<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key => "4";
    public string Description => "Update";
    private readonly IRepository<TDto> _repository;
    private readonly Func<TDto> _read;
    private readonly Action<TDto?> _print;
    public UpdateCommand(IRepository<TDto> repository, Func<TDto> read, Action<TDto?> print)
    { _repository = repository; _read = read; _print = print; }
    public void Execute()
    {
        int id = ConsoleInput.ReadInt("ID");
        if (_repository.GetById(id) == null) { Console.WriteLine("Запис не знайдено."); return; }
        var changed = _read();
        changed.Id = id;
        if (_repository.Update(changed)) _print(_repository.GetById(id));
        else Console.WriteLine("Запис не знайдено.");
    }
}
