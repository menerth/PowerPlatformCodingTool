using Microsoft.Extensions.Logging;

namespace PPCT.Tasks
{
    public class UpcomingGenericTask(ILogger<UpcomingGenericTask> log) : IPPCTTask
    {
        public Task<bool> Execute(CancellationToken ct)
        {
            log.LogWarning("This is an upcoming feature not yet available");
            return Task.FromResult(true);
        }
    }
}
