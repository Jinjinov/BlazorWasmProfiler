using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWasmProfiler;

internal class ComponentSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateSyntaxNodes { get; } = new List<ClassDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.BaseList?.Types.Any(t => t.Type.ToString().Contains("ComponentBase")) == true)
        {
            CandidateSyntaxNodes.Add(classDeclarationSyntax);
        }
    }
}