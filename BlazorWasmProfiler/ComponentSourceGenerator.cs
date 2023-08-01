using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace BlazorWasmProfiler;

[Generator]
internal class ComponentSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ComponentSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ComponentSyntaxReceiver receiver)
            return;

        MethodDeclarationSyntax newOnParametersSetMethod = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "OnParametersSet")
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.OverrideKeyword)))
            .WithAttributeLists(SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("RenderTimeTrackingAttribute")))
            )))
            .WithParameterList(SyntaxFactory.ParameterList())
            .WithBody(SyntaxFactory.Block());

        foreach (ClassDeclarationSyntax classDeclaration in receiver.CandidateSyntaxNodes)
        {
            SyntaxNode root = classDeclaration.SyntaxTree.GetRoot();

            bool hasOnParametersSetMethod = HasOnParametersSetMethod(classDeclaration);

            if (hasOnParametersSetMethod)
            {
                MethodDeclarationSyntax? methodOnParametersSet = GetOnParametersSetMethod(classDeclaration);

                if (methodOnParametersSet != null)
                {
                    MethodDeclarationSyntax updatedMethod = methodOnParametersSet.WithAttributeLists(SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("RenderTimeTrackingAttribute")))
                    )));

                    SyntaxNode newRoot = root.ReplaceNode(methodOnParametersSet, updatedMethod);

                    context.AddSource($"{classDeclaration.Identifier.ValueText}_Updated.cs", newRoot.NormalizeWhitespace().ToFullString());
                }
            }
            else
            {
                ClassDeclarationSyntax updatedClassDeclaration = classDeclaration.AddMembers(newOnParametersSetMethod);

                SyntaxNode newRoot = root.ReplaceNode(classDeclaration, updatedClassDeclaration);

                context.AddSource(classDeclaration.Identifier.Text + ".cs", newRoot.ToString());
            }
        }
    }

    private static bool HasOnParametersSetMethod(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(method => method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ProtectedKeyword))
                           && method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword))
                           && method.ReturnType.ToString() == "void"
                           && method.Identifier.ValueText == "OnParametersSet");
    }

    private static MethodDeclarationSyntax? GetOnParametersSetMethod(ClassDeclarationSyntax classDeclaration)
    {
        return classDeclaration.Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(method => method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ProtectedKeyword))
                           && method.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword))
                           && method.ReturnType.ToString() == "void"
                           && method.Identifier.ValueText == "OnParametersSet");
    }

    private static IMethodSymbol? GetOnParametersSetMethod(Compilation compilation, ClassDeclarationSyntax classDeclaration)
    {
        INamedTypeSymbol? classSymbol = compilation.GetSemanticModel(classDeclaration.SyntaxTree).GetDeclaredSymbol(classDeclaration);

        IMethodSymbol? method = classSymbol?.GetMembers("OnParametersSet").OfType<IMethodSymbol>().FirstOrDefault(m =>
            m.DeclaredAccessibility == Accessibility.Protected &&
            m.IsOverride &&
            m.Parameters.Length == 0 &&
            m.ReturnType.SpecialType == SpecialType.System_Void
        );

        return method;
    }
}
