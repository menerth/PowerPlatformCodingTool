# PPCT

Power Platform Coding Tool - An open-source tool to deploy Power Platform dependent assemblies with configuration in code.

Heavily inspired by spkl (https://github.com/scottdurow/SparkleXrm)

## Features

- Deployment of dependent assemblies to Power Platform based on configuration in code
- Generating configuration in code based on existing Power Platform environment
- Available as a .NET tool
- PPCT.Components - A small library to be added to your project to support PPCT (Attribute for plugin classes, no externl dependencies)
- Stores recently used connections in token cache, authorization via browser (no need to store credentials in configuration)

## Installation

```
dotnet tool install --global ppct
```

## Usage

### Initialize PPCT configuration in current directory
```
ppct -t init
```

- Update ppct.json with your Power Platform environment details, e.g.:
```json
{
  "SolutionPath": "YourSolution.sln",
  "NugetPackage": {
    "DataverseSolutionName": "YourPowerPlatformSolutionName",
    "NugetPackagePath": "yourproject\\bin\\outputPackages"
  }
}
```

  - Run decorate.bat to update your code with existing configuration
  - Run deploy.bat to deploy your assemblies to Power Platform based on configuration in code (similar approach as in spkl)

## Disclaimer

This is a work in progress and serves mainly as a tool me & my team are leveraging. Use at your own risk.

PPCT can currently handle only dependent assemblies, as it is not covered by spkl.

More features might be coming in the future.
