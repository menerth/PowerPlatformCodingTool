using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace PPCT
{
    public static class Roslyn
    {
        public static AttributeArgumentSyntax CreateAttributeArgument(string enumTypeName, string enumValueName)
        {
            return SyntaxFactory.AttributeArgument(
                SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(enumTypeName),
                SyntaxFactory.IdentifierName(enumValueName)));
        }

        public static AttributeArgumentSyntax CreateAttributeArgument(string stringValue)
        {
            return SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(
                SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(stringValue)));
        }

        public static AttributeArgumentSyntax CreateAttributeArgument(int intValue)
        {
            return SyntaxFactory.AttributeArgument(SyntaxFactory.LiteralExpression(
                SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(intValue)));
        }

        public static AttributeArgumentSyntax CreateNamedArgument(string name, string value)
        {
            return SyntaxFactory.AttributeArgument(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(name),
                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value))
                )
            );
        }

        public static AttributeArgumentSyntax CreateNamedArgument(string name, int value)
        {
            return SyntaxFactory.AttributeArgument(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(name),
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value))
                )
            );
        }

        public static AttributeArgumentSyntax CreateNamedArgument(string name, string enumTypeName, string enumValueName)
        {
            return SyntaxFactory.AttributeArgument(
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(name),
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(enumTypeName),
                    SyntaxFactory.IdentifierName(enumValueName))));
        }

        public static AttributeSyntax CreateAttribute(string attributeName, AttributeArgumentSyntax[] arguments, AttributeArgumentSyntax[] namedArguments, SyntaxTriviaList classLeadingTrivia)
        {
            var allArguments = arguments.ToList();
            if (namedArguments != null)
            {
                allArguments.AddRange(namedArguments);
            }

            var argumentList = SyntaxFactory.SeparatedList<AttributeArgumentSyntax>();

            if (allArguments.Count < 4)
            {
                argumentList = SyntaxFactory.SeparatedList(allArguments);
            }
            else
            {
                var keepOnLineIndexes = new [] { 0, 2, 3, 5, 6 };
                for (var i = 0; i < allArguments.Count; i++)
                {
                    if (keepOnLineIndexes.Contains(i))
                    {
                        argumentList = argumentList.Add(allArguments[i]);
                    }

                    else
                    {
                        var lt = SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed);
                        lt = lt.AddRange(classLeadingTrivia);
                        argumentList = argumentList.Add(allArguments[i].WithLeadingTrivia(lt));
                    }
                }
            }

            return SyntaxFactory.Attribute(SyntaxFactory.ParseName(attributeName))
                            .WithArgumentList(SyntaxFactory.AttributeArgumentList(argumentList));
        }

        public static SyntaxNode AddUsingDirective<T>(SyntaxNode root)
        {
            var usingName = SyntaxFactory.ParseName(typeof(T).Namespace);
            var usingDirective = SyntaxFactory.UsingDirective(usingName).NormalizeWhitespace().WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            var rootNode = root as CompilationUnitSyntax;

            if (!rootNode.Usings.Select(d => d.Name.ToString()).Any(u => u == usingName.ToString()))
            {
                rootNode = rootNode.AddUsings(usingDirective);

                var usings = rootNode.Usings.OrderBy(u => u.Name.ToString());
                rootNode = rootNode.WithUsings(SyntaxFactory.List(usings));
            }

            return (SyntaxNode)rootNode;
        }
    }
}
