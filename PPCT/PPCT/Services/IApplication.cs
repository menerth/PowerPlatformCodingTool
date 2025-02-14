namespace PPCT.Services
{
    public interface IApplication
    {
        Task<bool> Execute(CancellationToken token = default);
    }
}