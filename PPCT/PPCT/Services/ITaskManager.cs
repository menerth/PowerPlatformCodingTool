using PPCT.Models;
using PPCT.Tasks;

namespace PPCT.Services
{
    public interface ITaskManager
    {
        ICCPTTask GetTask(Enums.PPCTTask task);
    }
}