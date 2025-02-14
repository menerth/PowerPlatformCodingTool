using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PPCT.Components;

namespace PPCT
{
    public static class DataverseRegistrationAttributeTools
    {

        public static AttributeSyntax ToAtributeSyntax(this DataverseRegistrationAttribute attr, SyntaxTriviaList classLeadingTrivia, SyntaxTriviaList classTrailingTrivia)
        {
            return attr.RegistrationType switch
            {
                RegistrationTypeEnum.Plugin => ToAtributeSyntaxPlugin(attr, classLeadingTrivia),
                RegistrationTypeEnum.CustomApi => ToAtributeSyntaxCustomApi(attr, classLeadingTrivia),
                _ => throw new NotSupportedException(),
            };
        }

        private static AttributeSyntax ToAtributeSyntaxPlugin(DataverseRegistrationAttribute attr, SyntaxTriviaList classLeadingTrivia)
        {
            var requiredArguments = new List<AttributeArgumentSyntax>();
            var namedArguments = new List<AttributeArgumentSyntax>();

            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.Message));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.EntityLogicalName));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(typeof(StageEnum).Name, attr.Stage.ToString()));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(typeof(ExecutionModeEnum).Name, attr.ExecutionMode.ToString()));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.FilteringAttributes));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.Name));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.ExecutionOrder));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(typeof(IsolationModeEnum).Name, attr.IsolationMode.ToString()));
            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.Id));

            if (!string.IsNullOrEmpty(attr.Image1Name))
            {
                namedArguments.Add(Roslyn.CreateNamedArgument("Image1Type", "ImageTypeEnum", attr.Image1Type.ToString()));
                namedArguments.Add(Roslyn.CreateNamedArgument("Image1Name", attr.Image1Name));
                namedArguments.Add(Roslyn.CreateNamedArgument("Image1Attributes", attr.Image1Attributes));
            }

            if (!string.IsNullOrEmpty(attr.Image2Name))
            {
                namedArguments.Add(Roslyn.CreateNamedArgument("Image2Type", "ImageTypeEnum", attr.Image2Type.ToString()));
                namedArguments.Add(Roslyn.CreateNamedArgument("Image2Name", attr.Image2Name));
                namedArguments.Add(Roslyn.CreateNamedArgument("Image2Attributes", attr.Image2Attributes));
            }

            if (!string.IsNullOrEmpty(attr.Description))
            {
                namedArguments.Add(Roslyn.CreateNamedArgument("Description", attr.Description));
            }
            if (!string.IsNullOrEmpty(attr.UnSecureConfiguration))
            {
                namedArguments.Add(Roslyn.CreateNamedArgument(nameof(DataverseRegistrationAttribute.UnSecureConfiguration), attr.UnSecureConfiguration));
            }

            var attribute = Roslyn.CreateAttribute(
                typeof(DataverseRegistrationAttribute).Name,
                [.. requiredArguments],
                [.. namedArguments],
                classLeadingTrivia
                );

            return attribute;
        }

        private static AttributeSyntax ToAtributeSyntaxCustomApi(DataverseRegistrationAttribute attr, SyntaxTriviaList classLeadingTrivia)
        {
            var requiredArguments = new List<AttributeArgumentSyntax>();
            var namedArguments = new List<AttributeArgumentSyntax>();

            requiredArguments.Add(Roslyn.CreateAttributeArgument(attr.Message));

            var attribute = Roslyn.CreateAttribute(
                typeof(DataverseRegistrationAttribute).Name,
                [.. requiredArguments],
                [.. namedArguments],
                classLeadingTrivia
                );

            return attribute;
        }

    }
}
