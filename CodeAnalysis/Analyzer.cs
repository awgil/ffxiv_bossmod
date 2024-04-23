using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    private static readonly List<DiagnosticDescriptor> s_diagnostics = [];
    private static DiagnosticDescriptor Register(string title, string message)
    {
        var res = new DiagnosticDescriptor($"VBM{s_diagnostics.Count + 1:d3}", title, message, "Custom rules", DiagnosticSeverity.Error, true);
        s_diagnostics.Add(res);
        return res;
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [.. s_diagnostics];

    private static readonly DiagnosticDescriptor RuleNoMutableStatics = Register("Modules and components should not contain mutable statics",
        "Field {0} of component or module {1} is a mutable static, which introduces a risk of different instances of modules affecting each other");
    private static readonly DiagnosticDescriptor RuleNoBitmaskProperties = Register("Bitmasks should not be exposed as read/write properties",
        "Property {0} of type {1} is a read/write bitmask, which introduces a risk of mutating a temporary");

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNoMutableStatics, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeNoBitmaskProperties, SymbolKind.Property);
    }

    private static void AnalyzeNoMutableStatics(SymbolAnalysisContext context)
    {
        if (context.Symbol is INamedTypeSymbol ns && ns.TypeKind == TypeKind.Class && IsSameOrDerivedFrom(ns.BaseType, "BossComponent", "BossModule"))
        {
            foreach (var m in ns.GetMembers().Where(IsMutableStaticField))
            {
                context.ReportDiagnostic(Diagnostic.Create(RuleNoMutableStatics, m.Locations[0], m.Name, ns.Name));
            }
        }
    }

    private static void AnalyzeNoBitmaskProperties(SymbolAnalysisContext context)
    {
        if (context.Symbol is IPropertySymbol p && !p.IsReadOnly && !p.IsWriteOnly && !p.ReturnsByRef && p.Type.Name == "BitMask")
        {
            if (p.ContainingType?.Name == "BitMatrix")
                return; // this is a hack; this is really a quite bad API that is quite risky and needs to be redesigned...
            context.ReportDiagnostic(Diagnostic.Create(RuleNoBitmaskProperties, p.Locations[0], p.Name, p.ContainingType?.Name));
        }
    }

    private static bool IsSameOrDerivedFrom(INamedTypeSymbol? symbol, params string[] bases)
    {
        while (symbol != null)
        {
            if (bases.Contains(symbol.Name))
                return true;
            symbol = symbol.BaseType;
        }
        return false;
    }

    private static bool IsMutableStaticField(ISymbol symbol)
    {
        if (symbol.Kind != SymbolKind.Field || !symbol.IsStatic)
            return false;
        var cast = (IFieldSymbol)symbol;
        return !cast.IsReadOnly && !cast.IsConst;
    }
}
