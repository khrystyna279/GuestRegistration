using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;

namespace GuestRegistration.ConsoleApp.Commands;

internal sealed class DeleteCommand<TDto> : IConsoleCommand where TDto : EntityDto
{
    public string Key => "5";
    public string Description => "Delete";
    private readonly IRepository<TDto> _repository;
    private readonly string? _warning;
    public DeleteCommand(IRepository<TDto> repository, string? warning)
    { _repository = repository; _warning = warning; }
    public void Execute()
    {
        int id = ConsoleInput.ReadInt("ID");
        if (_warning != null)
        {
            Console.WriteLine(_warning);
            if (!ConsoleInput.ReadBool("Видалити")) return;
        }
        Console.WriteLine(_repository.Delete(id) ? "Готово." : "Запис не знайдено.");
    }
}
