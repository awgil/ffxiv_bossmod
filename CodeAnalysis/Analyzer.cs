using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeAnalysis;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Analyzer : DiagnosticAnalyzer
{
    private static readonly List<DiagnosticDescriptor> s_diagnostics = [];
    private static DiagnosticDescriptor Register(string title, string message, DiagnosticSeverity severity = DiagnosticSeverity.Error)
    {
        var res = new DiagnosticDescriptor($"VBM{s_diagnostics.Count + 1:d3}", title, message, "Custom rules", severity, true);
        s_diagnostics.Add(res);
        return res;
    }
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [.. s_diagnostics];

    private static readonly DiagnosticDescriptor RuleNoMutableStatics = Register("Modules and components should not contain mutable statics",
        "Field {0} of component or module {1} is a mutable static, which introduces a risk of different instances of modules affecting each other");
    private static readonly DiagnosticDescriptor RuleNoBitmaskProperties = Register("Bitmasks should not be exposed as read/write properties",
        "Property {0} of type {1} is a read/write bitmask, which introduces a risk of mutating a temporary");
    private static readonly DiagnosticDescriptor RuleNoEmptyFirstLine = Register("First line of the file should not be empty", "Empty first line is pointless", DiagnosticSeverity.Warning);
    private static readonly DiagnosticDescriptor RuleUseSingleLineFindSlot = Register("Conditional can be inlined", "Use TryFindSlot instead of testing against 0", DiagnosticSeverity.Warning);
    private static readonly DiagnosticDescriptor RuleNoRealDatetimeInComponents = Register("Use of DateTime.Now in boss module", "DateTime.Now will behave unexpectedly in replays. Use WorldState.CurrentTime instead", DiagnosticSeverity.Error);
    private static readonly DiagnosticDescriptor RuleNoRefTypesInHintFuncs = Register("Reference type captured in closure", "This point test function captures a reference of type {0}, which might be modified before the function is called", DiagnosticSeverity.Error);
    public static readonly DiagnosticDescriptor RuleInternalNamesForOptions = Register("Bad arguments to AddOption", "Second argument ({1}) should match variant name ({0})", DiagnosticSeverity.Error);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNoMutableStatics, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeNoBitmaskProperties, SymbolKind.Property);
        context.RegisterSyntaxTreeAction(AnalyzeNoEmptyFirstLine);
        context.RegisterSyntaxNodeAction(AnalyzeUseInlineFindSlot, SyntaxKind.Block);
        context.RegisterSyntaxNodeAction(AnalyzeNoRealDatetime, SyntaxKind.Block);
        context.RegisterSyntaxNodeAction(AnalyzeNoRefsInHints, SyntaxKind.Block);
        //context.RegisterSyntaxNodeAction(AnalyzeOptionInternalNames, SyntaxKind.Block);
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

    private static void AnalyzeNoEmptyFirstLine(SyntaxTreeAnalysisContext context)
    {
        var leadingTrivia = context.Tree.GetRoot().GetLeadingTrivia();
        if (leadingTrivia.Count == 0)
            return;
        var firstTrivia = leadingTrivia[0].ToFullString();
        if (firstTrivia.Length > 0 && firstTrivia[0] is '\r' or '\n')
            context.ReportDiagnostic(Diagnostic.Create(RuleNoEmptyFirstLine, context.Tree.GetLocation(leadingTrivia[0].FullSpan)));
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

    private static void AnalyzeUseInlineFindSlot(SyntaxNodeAnalysisContext context)
    {
        var findSlots = context.Compilation.GetTypeByMetadataName("BossMod.PartyState")?
            .GetMembers()
            .Where(m => m.Name == "FindSlot")
            .ToList();
        if (findSlots == null)
            return;

        foreach (var decl in context.Node.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            foreach (var variable in decl.Declaration.Variables)
            {
                if (variable.Initializer?.Value is InvocationExpressionSyntax i)
                {
                    var funcSymbol = context.SemanticModel.GetSymbolInfo(i.Expression).Symbol;
                    if (funcSymbol != null && findSlots.Contains(funcSymbol))
                    {
                        var filt = new FilterConditionals(variable.Identifier);
                        filt.Visit(context.Node);
                        foreach (var loc in filt.Locations)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(RuleUseSingleLineFindSlot, loc.GetLocation()));
                        }
                    }
                }
            }
        }
    }

    private static void AnalyzeNoRefsInHints(SyntaxNodeAnalysisContext context)
    {
        foreach (var node in context.Node.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (!IsHintZoneFn(context, node))
                continue;

            if (node.ArgumentList.Arguments.First().Expression is LambdaExpressionSyntax lam)
            {
                var anal = context.SemanticModel.AnalyzeDataFlow(lam.Body);

                foreach (var captured in anal.CapturedInside)
                    if (captured is IParameterSymbol p && p.Type.IsReferenceType)
                        context.ReportDiagnostic(Diagnostic.Create(RuleNoRefTypesInHintFuncs, lam.GetLocation(), $"{p.Type}"));
            }
        }
    }

    private static void AnalyzeOptionInternalNames(SyntaxNodeAnalysisContext context)
    {
        foreach (var node in context.Node.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (node.Expression is MemberAccessExpressionSyntax mem && mem.Name.ToString() == "AddOption")
            {
                var argList = node.ArgumentList.Arguments;

                // only one arg
                if (argList.Count < 2)
                    continue;

                if (argList[0].Expression is MemberAccessExpressionSyntax mem2)
                {
                    var variantName = mem2.Name.ToString();
                    var internalName = (argList[1].Expression as LiteralExpressionSyntax)?.Token.ValueText ?? "";
                    if (variantName != internalName)
                        context.ReportDiagnostic(Diagnostic.Create(RuleInternalNamesForOptions, node.ArgumentList.GetLocation(), variantName, internalName));
                }
            }
        }
    }

    private static bool IsHintZoneFn(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node)
    {
        if (node.Expression is MemberAccessExpressionSyntax mem)
        {
            if (mem.Name.ToString() == "AddForbiddenZone")
                return true;

            if (mem.Expression is MemberAccessExpressionSyntax parent)
                return parent.Name.ToString() == "GoalZones" && mem.Name.ToString() == "Add";
        }

        return false;
    }

    private static void AnalyzeNoRealDatetime(SyntaxNodeAnalysisContext context)
    {
        if (context.ContainingSymbol?.ContainingType is { } ty && IsSameOrDerivedFrom(ty, "BossComponent", "BossModule"))
        {
            foreach (var node in context.Node.DescendantNodes().OfType<MemberAccessExpressionSyntax>())
            {
                if (node.Expression.ToString() == "DateTime" && node.Name.ToString() is "Now" or "Today")
                    context.ReportDiagnostic(Diagnostic.Create(RuleNoRealDatetimeInComponents, node.GetLocation()));
            }
        }
    }
}

class FilterConditionals(SyntaxToken symbol) : CSharpSyntaxWalker
{
    public List<SyntaxNode> Locations = [];

    public override void VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        switch ((node.Left, node.Right))
        {
            case (IdentifierNameSyntax ivar, LiteralExpressionSyntax inum):
                if (ivar.Identifier.Text == symbol.Text && IsPatternBogus(inum.Token))
                    Locations.Add(node);
                break;
            case (LiteralExpressionSyntax inum, IdentifierNameSyntax ivar):
                if (ivar.Identifier.Text == symbol.Text && IsPatternBogus(inum.Token))
                    Locations.Add(node);
                break;
            default:
                Visit(node.Left);
                Visit(node.Right);
                break;
        }
    }

    public override void VisitIsPatternExpression(IsPatternExpressionSyntax node)
    {
        if (node.Expression is IdentifierNameSyntax i && i.Identifier.Text == symbol.Text)
        {
            if (node.Pattern.DescendantNodes().All(d => d is RelationalPatternSyntax r && IsPatternBogus(r.Expression)))
                Locations.Add(node);
        }
    }

    private static bool IsPatternBogus(ExpressionSyntax s) => s.ToString() is "0" or "-1";
    private static bool IsPatternBogus(SyntaxToken t) => t.Text is "0" or "-1";
}
