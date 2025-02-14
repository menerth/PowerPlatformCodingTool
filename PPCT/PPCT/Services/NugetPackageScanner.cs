using Microsoft.Extensions.Logging;
using NuGet.Frameworks;
using NuGet.Packaging;
using PPCT.Components;
using PPCT.Models;
using System.Reflection;
using static NuGet.Frameworks.FrameworkConstants;

namespace PPCT.Services
{
    public class NugetPackageScanner(ILogger<NugetPackageScanner> log)
    {
        private readonly ILogger<NugetPackageScanner> _log = log;
        private readonly NuGetFramework _netFramework = new(FrameworkIdentifiers.Net, new Version(4, 6, 2, 0));
        private string[] IgnoredNamespaces = ["System", "Microsoft", "Newtonsoft", "NuGet", "PPCT", "Azure", "AutoMapper"];

        public List<NugetAssemblyPackage> ScanPackages(NugetFileConfig config, int nugetPackageTypeCode)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), config.NugetPackagePath);
            _log.LogTrace("Searching for packages...");

            var packagePaths = GetPackagesPaths(path);

            var scannedPackages = new List<NugetAssemblyPackage>();

            foreach (var item in packagePaths)
            {
                var scannedPackage = ScanPackageInternal(item);
                scannedPackage.EntityTypeCode = nugetPackageTypeCode;

                scannedPackages.Add(scannedPackage);
            }

            return scannedPackages;
        }

        private NugetAssemblyPackage ScanPackageInternal(string packagePath)
        {
            var nugetBinary = File.ReadAllBytes(packagePath);

            using FileStream inputStream = new(packagePath, FileMode.Open);
            using PackageArchiveReader reader = new(inputStream);
            NuspecReader nuspec = reader.NuspecReader;

            _log.LogInformation("Package: {id} v{version}", nuspec.GetId(), nuspec.GetVersion());

            var items = (reader?.GetLibItems()?.FirstOrDefault(x => x.TargetFramework == _netFramework)?.Items) ?? throw new Exception("Target framework not found in NuGet package!!!");

            var filteredItems = items.Where(entry => entry.EndsWith(".dll") && !IgnoredNamespaces.Any(ign => entry.Split("/").Last().StartsWith(ign, StringComparison.InvariantCultureIgnoreCase)));

            if (!filteredItems.Any())
            {
                throw new Exception("Target NuGet has no assembly with plugin code...");
            }

            var packageContent = new NugetAssemblyPackage()
            {
                Name = nuspec.GetId(),
                Version = nuspec.GetVersion().ToString(),
            };

            foreach (var item in filteredItems)
            {
                var assemblyName = item.Split("/").Last();
                _log.LogTrace("Loading assembly {assemblyName}...", assemblyName);
                var stream = reader.GetStream(item);

                var assembly = Reflection.LoadAssemblyFromStream(stream, assemblyName);

                var pluginTypes = Reflection.GetPluginTypes(assembly, true);

                if (!pluginTypes.Any())
                {
                    continue;
                }

                var assemblyContent = new NugetAssembly()
                {
                    //removing ".dll" from assembly name
                    AssemblyName = assemblyName[..^4],
                };

                foreach (var pluginType in pluginTypes)
                {
                    var pluginContent = new RegisterableClass()
                    {
                        ClassType = pluginType
                    };

                    var typeAttributes = GetDataverseRegistrationAttributes<DataverseRegistrationAttribute>(pluginType);

                    if (typeAttributes.Any())
                    {
                        pluginContent.Attributes.AddRange(typeAttributes);

                        var duplicateSteps = pluginContent.Attributes.GroupBy(x => x.Name).Where(x => x.Count() > 1).Select(x => x.Key);

                        if (duplicateSteps.Any())
                        {
                            throw new Exception($"Attribute(s) with duplicated names were detected: {string.Join(";", duplicateSteps)}");
                        }

                        if (pluginContent.Attributes.DistinctBy(x => x.RegistrationType).Count() > 1)
                        {
                            throw new Exception($"Class {pluginType.Name} in {assemblyName} has multiple registration types");
                        }
                    }

                    assemblyContent.RegisterableClasses.Add(pluginContent);
                }

                packageContent.Assemblies.Add(assemblyContent);
            }

            var duplicatedCustomAPIs = packageContent.Assemblies.SelectMany(a => a.RegisterableClasses)
                .SelectMany(rc => rc.Attributes)
                .Where(attr => attr.RegistrationType == RegistrationTypeEnum.CustomApi)
                .GroupBy(attr => attr.Message)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicatedCustomAPIs.Any())
            {
                throw new Exception($"Duplicated Custom API messages detected:\n{string.Join("\n", duplicatedCustomAPIs)}");
            }

            packageContent.Base64Content = ConvertFileToBase64(inputStream);
            packageContent.Base64Content = Convert.ToBase64String(nugetBinary);

            if (packageContent.Assemblies.Count == 0)
            {
                throw new Exception("No plugin classes found in target NuGet package!!!");
            }

            var registrationTypeCounts = packageContent.Assemblies
            .SelectMany(a => a.RegisterableClasses)
            .SelectMany(rc => rc.Attributes)
            .GroupBy(attr => attr.RegistrationType)
            .Select(g => new { RegistrationType = g.Key, Count = g.Count() })
            .ToList();

            _log.LogInformation("Package analysis completed successfully!");
            _log.LogInformation("Package content: {assemblyCount} assemblies with registerable code", packageContent.Assemblies.Count);

            foreach (var item in registrationTypeCounts)
            {
                _log.LogInformation("{registrationType} registration type: {count} registrations", item.RegistrationType, item.Count);
            }

            return packageContent;
        }

        private string[] GetPackagesPaths(string searchPath)
        {
            var packages = Directory.GetFiles(searchPath, "*.nupkg");

            if (packages.Length == 0)
            {
                throw new Exception("No NuGet packages found in configured folder!!!");
            }
            else
            {
                _log.LogInformation("Found {count} NuGet package(s)", packages.Length);

                return packages;
            }
        }

        static string ConvertFileToBase64(FileStream fileStream)
        {
            // Read the file into a byte array
            byte[] fileBytes;

            using (BinaryReader binaryReader = new(fileStream))
            {
                fileBytes = binaryReader.ReadBytes((int)fileStream.Length);
            }

            // Convert the byte array to a Base64 string
            return Convert.ToBase64String(fileBytes);
        }

        private static ConstructorInfo GetConstructor(CustomAttributeData data)
        {
            var attributeType = data.AttributeType;
            var constructorArgs = data.ConstructorArguments.Select(arg => arg.Value).ToArray();
            var constructorArgsTypes = data.ConstructorArguments.Select(arg => arg.ArgumentType).ToArray();
            var namedInput = data.NamedArguments.ToDictionary(arg => arg.MemberName, arg => arg.TypedValue.Value);

            // Find the correct constructor by matching argument types
            var constructorInfo = attributeType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var parameters = c.GetParameters();
                    if (parameters.Length != constructorArgsTypes.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType != constructorArgsTypes[i])
                        {
                            return false;
                        }
                    }

                    return true;
                });

            return constructorInfo;
        }

        private static IEnumerable<TCustom> GetDataverseRegistrationAttributes<TCustom>(Type type) where TCustom : Attribute
        {
            var attributes = type.CustomAttributes.Where(x => x.AttributeType == typeof(TCustom));

            foreach (var attr in attributes)
            {
                var constructor = GetConstructor(attr);

                if (constructor != null)
                {
                    var requiredInput = attr.ConstructorArguments.Select(x => x.Value).ToArray();
                    var namedInput = attr.NamedArguments.ToDictionary(x => x.MemberName, x => x.TypedValue);
                    var instance = (TCustom)constructor.Invoke(requiredInput);

                    if (instance is TCustom myType)
                    {
                        var instanceType = instance.GetType();
                        foreach (var prop in namedInput)
                        {
                            instanceType.InvokeMember(prop.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, instance, [Convert.ChangeType(prop.Value.Value, prop.Value.ArgumentType)]);
                        }
                        yield return instance;
                    }
                }
            }

        }
    }
}
