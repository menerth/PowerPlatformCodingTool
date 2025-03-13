using Microsoft.Extensions.Logging;
using PPCT.Models;

namespace PPCT.Services
{
    public class Application(ITaskManager manager, AppInput args, ILogger<Application> log) : IApplication
    {
        private readonly ILogger<Application> _log = log;
        private readonly AppInput _args = args;
        private readonly ITaskManager _manager = manager;

        public async Task<bool> Execute(CancellationToken ct = default)
        {
            try
            {
                return await _manager.GetTask(_args).Execute(ct);
            }
            catch (Exception ex)
            {
                _log.LogError("An error occurred while executing the task:\n{error}", ex.Message);
                return false;
            }
            
        }
    }
}
