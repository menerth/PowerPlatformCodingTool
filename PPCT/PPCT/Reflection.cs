using Microsoft.Xrm.Sdk;
using System.Reflection;

namespace PPCT
{
    public class Reflection
    {
        public static Assembly LoadAssemblyFromStream(Stream stream, string assemblyName)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            try
            {
                var assembly = Assembly.Load(memoryStream.ToArray());
                return assembly;
            }
            catch (BadImageFormatException)
            {
                throw new Exception($"Error loading assembly {assemblyName} in NuGet package");
            }
        }

        public static IEnumerable<Type> GetPluginTypes(Assembly assembly, bool nuGetMode = false)
        {
            var pluginTypes = assembly.ExportedTypes.Where(x => x.GetInterfaces().FirstOrDefault(y => y.Name == typeof(IPlugin).Name) != null && !x.IsAbstract);
            if (!nuGetMode)
            {
                return pluginTypes;
            }

            var workflowTypes = assembly.ExportedTypes.Where(x => x?.BaseType?.FullName == "System.Activities.CodeActivity");

            if (workflowTypes.Any())
            {
                throw new Exception("Code Activities are not supported in Dependent Assemblies");
            }

            return pluginTypes;

        }
    }
}
