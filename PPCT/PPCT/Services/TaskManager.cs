using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PPCT.Models;
using PPCT.Tasks;

namespace PPCT.Services
{
    public class TaskManager(IServiceProvider serviceProvider, ILogger<TaskManager> log) : ITaskManager
    {
        public IPPCTTask GetTask(AppInput appInput)
        {
            log.LogTrace("Task for execution: {task}", appInput.TaskFlag);

            return serviceProvider.GetRequiredKeyedService<IPPCTTask>(appInput.TaskFlag);
        }
    }
}
