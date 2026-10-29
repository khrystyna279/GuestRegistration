using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Commands;

internal sealed class ReadByIdCommand<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key => "3";
    public string Description => "Read by ID";
    private readonly IRepository<TDto> _repository;
    private readonly Action<TDto?> _print;
    public ReadByIdCommand(IRepository<TDto> repository, Action<TDto?> print)
    { _repository = repository; _print = print; }
    public void Execute() => _print(_repository.GetById(ConsoleInput.ReadInt("ID")));
}
