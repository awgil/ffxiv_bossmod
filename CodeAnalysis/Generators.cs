using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CodeAnalysis;

[Generator]
public class StrategiesGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    private static readonly SymbolDisplayFormat Qualified = new(globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted, typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces, genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters, miscellaneousOptions: SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers | SymbolDisplayMiscellaneousOptions.UseSpecialTypes);

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
        {
            throw new Exception("SyntaxReceiver is missing");
        }

        //throw new Exception($"SyntaxReceiver found nodes: {string.Join(", ", receiver.Symbols.Select(s => s.ToDisplayString()))}");

        foreach (var declared in receiver.Symbols)
        {
            var syn = GenerateValuesStruct(declared);

            context.AddSource($"{declared.ToDisplayString(Qualified)}.g.cs", SourceText.From(syn.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
        }
    }

    private SyntaxNode GenerateValuesStruct(INamedTypeSymbol declared) => SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(declared.ContainingNamespace.ToDisplayString(Qualified)))
        .WithMembers([
            SyntaxFactory.StructDeclaration(SyntaxFactory.Identifier($"{declared.ContainingType.Name}Strategy"))
                .WithModifiers([SyntaxFactory.Token(SyntaxKind.PublicKeyword)])
                .WithMembers([..declared.GetMembers().OfType<IFieldSymbol>()
                    .Select(field =>
                        SyntaxFactory.FieldDeclaration(
                            SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName($"TypedStrategyTrack<{field.Type.ToDisplayString(Qualified)}>"), [SyntaxFactory.VariableDeclarator(field.Name.ToString())])
                        ).WithModifiers([SyntaxFactory.Token(SyntaxKind.PublicKeyword)])
                )])
            ]);

    class SyntaxReceiver : ISyntaxContextReceiver
    {
        public readonly List<INamedTypeSymbol> Symbols = [];

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is TypeDeclarationSyntax ty && ty.AttributeLists.Count > 0)
            {
                if (context.SemanticModel.GetDeclaredSymbol(context.Node) is INamedTypeSymbol sym)
                {
                    if (sym.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "BossMod.Autorotation.StrategyAttribute"))
                    {
                        Symbols.Add(sym);
                    }
                }
            }
        }
    }
}
