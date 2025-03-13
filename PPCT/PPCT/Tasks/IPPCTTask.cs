namespace PPCT.Tasks
{
    public interface IPPCTTask
    {
        Task<bool> Execute(CancellationToken ct);
    }
}