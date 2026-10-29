using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Commands;

internal sealed class CreateCommand<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key => "1";
    public string Description => "Create";
    private readonly IRepository<TDto> _repository;
    private readonly Func<TDto> _read;
    private readonly Action<TDto?> _print;
    public CreateCommand(IRepository<TDto> repository, Func<TDto> read, Action<TDto?> print)
    { _repository = repository; _read = read; _print = print; }
    public void Execute() => _print(_repository.Create(_read()));
}
