namespace PPCT.Tasks
{
    public interface ICCPTTask
    {
        Task<bool> Execute(CancellationToken ct);
    }
}