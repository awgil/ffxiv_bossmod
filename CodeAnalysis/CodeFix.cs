using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace CodeAnalysis;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public class VBMProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [Analyzer.RuleInternalNamesForOptions.Id];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();
        var argList = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!.AncestorsAndSelf().OfType<ArgumentListSyntax>().First();

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Clean up argument list",
                createChangedDocument: c => FixArgumentList(context.Document, argList, c),
                equivalenceKey: "Clean up argument list"
            ),
            diagnostic
        );
    }

    private static async Task<Document> FixArgumentList(Document document, ArgumentListSyntax argList, CancellationToken cancellationToken)
    {
        static string? asStringLit(ExpressionSyntax syn) => syn.IsKind(SyntaxKind.StringLiteralExpression) ? (syn as LiteralExpressionSyntax)!.Token.ValueText : null;
        static bool isStringLit(ExpressionSyntax syn) => asStringLit(syn) != null;

        var args = argList.Arguments;

        if (args.Count == 0)
            return document;

        var variantName = (args[0].Expression as MemberAccessExpressionSyntax)!.Name.ToString();
        var variantNameArg = SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(variantName)));

        var numStringArgs = args.Take(3).Count(a => isStringLit(a.Expression));

        // internal and display name both specified, make sure internal name is correct (it doesn't matter anyway since we're deleting it)
        if (numStringArgs >= 2)
            args = args.Replace(args[1], variantNameArg);

        // only display name is present, add variant name
        if (numStringArgs == 1)
            args = args.Insert(1, variantNameArg);

        var synNew = argList.WithArguments(args);
        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);

        var newRoot = oldRoot!.ReplaceNode(argList, synNew);
        return document.WithSyntaxRoot(newRoot);
    }
}
