using Microsoft.CodeAnalysis;
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

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
        {
            throw new Exception("SyntaxReceiver is missing");
        }

        //throw new Exception($"SyntaxReceiver found nodes: {string.Join(", ", receiver.Symbols.Select(s => s.ToDisplayString()))}");

        foreach (var declared in receiver.Symbols)
        {
            var name = declared.Name;
            var cnt = declared.ContainingType;

            context.AddSource($"{name}.g.cs", SourceText.From($@"
                using BossMod.Autorotation;

                namespace {declared.ContainingNamespace.Name};

                partial class {cnt.Name} {{
                    partial struct {name} : IStrategy<{name}> {{
                        public readonly {name} FromValues(StrategyValues values) => throw new NotImplementedException();
                    }}
                }}
", Encoding.UTF8));
        }
    }

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
