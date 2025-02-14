using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using PPCT.Components;

namespace PPCT.Services
{
    public class SolutionProcessor(ILogger<SolutionProcessor> log)
    {
        private readonly ILogger<SolutionProcessor> _log = log;

        public async Task ScanSolution(string relativePath, Dictionary<string, IEnumerable<DataverseRegistrationAttribute>> attributes)
        {
            var solutionPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);

            _log.LogInformation("Scanning for solution in {path}...", solutionPath);

            using var workspace = MSBuildWorkspace.Create();
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            foreach (var projectId in solution.ProjectIds)
            {
                var project = solution.GetProject(projectId);
                _log.LogInformation("Running for project: {name}", project.Name);
                foreach (var documentId in project.DocumentIds)
                {
                    var document = solution.GetDocument(documentId);
                    if (document.SourceCodeKind != SourceCodeKind.Regular)
                    {
                        continue;
                    }

                    var doc = document;

                    _log.LogTrace("Found document: {name} at {path}", doc.Name, doc.FilePath);
                    var root = await doc.GetSyntaxRootAsync();
                    var semanticModel = await doc.GetSemanticModelAsync();

                    var update = false;
                    var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                        .Where(c => c.Parent is NamespaceDeclarationSyntax);

                    foreach (var classDeclaration in classDeclarations)
                    {
                        if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol || classSymbol.IsAbstract || !ImplementsInterface(classSymbol, typeof(IPlugin).Name))
                        {
                            continue;
                        }
                        var pluginTypeName = $"{(classDeclaration.Parent as NamespaceDeclarationSyntax).Name}.{classSymbol.Name}";

                        var classAttributes = attributes.TryGetValue(pluginTypeName, out var attrs) ? attrs : [];

                        _log.LogInformation("Found {count} attributes for plugin: {name}", classAttributes.Count(), pluginTypeName);

                        var newCd = classDeclaration.WithAttributeLists(new SyntaxList<AttributeListSyntax>());

                        var classLeadingTrivia = classDeclaration.GetLeadingTrivia();
                        var classTrailingTrivia = classDeclaration.GetTrailingTrivia();

                        var syntAttrs = classAttributes.Select(x => x.ToAtributeSyntax(classLeadingTrivia, classTrailingTrivia)).ToArray();

                        var attributeLists = syntAttrs.Select(x => SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(x))
                            .WithLeadingTrivia(classLeadingTrivia)
                            .WithTrailingTrivia(classTrailingTrivia));

                        newCd = newCd.AddAttributeLists(attributeLists.ToArray());

                        root = root.ReplaceNode(classDeclaration, newCd);

                        root = Roslyn.AddUsingDirective<DataverseRegistrationAttribute>(root);

                        update = true;
                    }

                    if (update)
                    {
                        _log.LogInformation("Updating document: {name}", doc.Name);
                        doc = doc.WithSyntaxRoot(root);
                        project = doc.Project;
                        solution = project.Solution;
                    }

                }
            }
            workspace.TryApplyChanges(solution);
        }

        private static bool ImplementsInterface(INamedTypeSymbol typeSymbol, string interfaceName)
        {
            return typeSymbol.AllInterfaces.Any(i => i.Name == interfaceName) ||
                   (typeSymbol.BaseType != null && ImplementsInterface(typeSymbol.BaseType, interfaceName));
        }
    }
}
