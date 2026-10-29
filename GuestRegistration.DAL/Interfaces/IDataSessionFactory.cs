namespace GuestRegistration.DAL.Interfaces;

public interface IDataSessionFactory
{
    IDataSession Open(bool transactional = false);
}
