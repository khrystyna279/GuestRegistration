using GuestRegistration.DAL.Data;
using GuestRegistration.DAL.Interfaces;
using GuestRegistration.DTO;
using Microsoft.EntityFrameworkCore;

namespace GuestRegistration.DAL.Repositories;

public abstract class RepositoryBase<TEntity, TDto> : IRepository<TDto>
    where TEntity : class
    where TDto : EntityDto
{
    // The caller owns and disposes the injected context.
    protected AppDbContext Context { get; }
    private DbSet<TEntity> Entities => Context.Set<TEntity>();

    protected RepositoryBase(AppDbContext context) => Context = context;

    protected abstract TDto ToDto(TEntity entity);
    protected abstract TEntity ToEntity(TDto dto);

    public TDto Create(TDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Id != 0) throw new ArgumentException("Для нового запису Id має бути 0.");
        var entity = ToEntity(dto);
        Entities.Add(entity);
        Context.SaveChanges();
        Context.Entry(entity).Reload();
        return ToDto(entity);
    }

    public List<TDto> GetAll() => Entities.AsNoTracking()
        .OrderBy(entity => EF.Property<int>(entity, "Id"))
        .ToList().Select(ToDto).ToList();

    public TDto? GetById(int id)
    {
        var entity = Entities.AsNoTracking().FirstOrDefault(entity => EF.Property<int>(entity, "Id") == id);
        return entity == null ? null : ToDto(entity);
    }

    public bool Update(TDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var existing = Entities.Find(dto.Id);
        if (existing == null) return false;
        Context.Entry(existing).CurrentValues.SetValues(ToEntity(dto));
        Context.SaveChanges();
        Context.Entry(existing).Reload();
        return true;
    }

    public bool Delete(int id)
    {
        var entity = Entities.Find(id);
        if (entity == null) return false;
        Entities.Remove(entity);
        Context.SaveChanges();
        return true;
    }
}
