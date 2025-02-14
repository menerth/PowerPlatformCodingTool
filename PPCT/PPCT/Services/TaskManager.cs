using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPCT.Tasks;
using static PPCT.Models.Enums;

namespace PPCT.Services
{
    public class TaskManager(IServiceProvider serviceProvider, ILogger<TaskManager> log) : ITaskManager
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<TaskManager> _log = log;

        public ICCPTTask GetTask(PPCTTask task)
        {
            _log.LogTrace("Task for execution: {task}", task);
            return task switch
            {
                PPCTTask.Init => _serviceProvider.GetRequiredKeyedService<ICCPTTask>(task),
                PPCTTask.Deploy => _serviceProvider.GetRequiredKeyedService<ICCPTTask>(task),
                PPCTTask.Extract => _serviceProvider.GetRequiredKeyedService<ICCPTTask>(task),
                _ => null,
            };
        }
    }
}
