using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Commands;

internal sealed class ReadAllCommand<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key => "2";
    public string Description => "Read all";
    private readonly IRepository<TDto> _repository;
    private readonly Action<TDto?> _print;
    public ReadAllCommand(IRepository<TDto> repository, Action<TDto?> print)
    { _repository = repository; _print = print; }
    public void Execute()
    {
        var records = _repository.GetAll();
        if (records.Count == 0) Console.WriteLine("Таблиця порожня.");
        foreach (var dto in records) _print(dto);
    }
}
