namespace PPCT.Services
{
    public interface IConfigurationFileLoader
    {
        T LoadConfigurationFile<T>();
    }
}