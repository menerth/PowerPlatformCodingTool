using PPCT.Models;
using PPCT.Tasks;

namespace PPCT.Services
{
    public interface ITaskManager
    {
        IPPCTTask GetTask(AppInput appInput);
    }
}