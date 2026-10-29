namespace GuestRegistration.DTO;

public abstract class EntityDto
{
    public int Id { get; set; }
    public DateTime RowInsertTime { get; set; }
    public DateTime UpdateTime { get; set; }
}
