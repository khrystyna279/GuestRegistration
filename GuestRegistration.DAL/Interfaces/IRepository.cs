using GuestRegistration.DTO;

namespace GuestRegistration.DAL.Interfaces;

public interface IRepository<TDto> where TDto : EntityDto
{
    TDto Create(TDto dto);
    List<TDto> GetAll();
    TDto? GetById(int id);
    bool Update(TDto dto);
    bool Delete(int id);
}
