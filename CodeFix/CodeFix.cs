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
    public override ImmutableArray<string> FixableDiagnosticIds => [Analyzer.RuleInternalNamesForOptions.Id, Analyzer.RuleUseModuleInitializer.Id];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (diagnostic.Id == Analyzer.RuleInternalNamesForOptions.Id)
            {
                var argList = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!.AncestorsAndSelf().OfType<ArgumentListSyntax>().First();
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Clean up argument list",
                        createChangedDocument: c => FixOptionDefinitionArgList(context.Document, argList, c),
                        equivalenceKey: "clean_up_arg_list"
                    ),
                    diagnostic
                );
            }

            if (diagnostic.Id == Analyzer.RuleUseModuleInitializer.Id)
            {
                var decl = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!;

                if (decl.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() is { } cctor)
                {
                    context.RegisterCodeFix(CodeAction.Create(
                        title: "Use module initializer",
                        createChangedDocument: c => FixConstructor(context.Document, cctor, c),
                        equivalenceKey: "mod_ctor"
                    ), diagnostic);
                }
                else if (decl.FirstAncestorOrSelf<ClassDeclarationSyntax>() is { } cdecl)
                {
                    context.RegisterCodeFix(CodeAction.Create(
                        title: "Use module initializer",
                        createChangedDocument: c => FixPrimaryConstructor(context.Document, cdecl, c),
                        equivalenceKey: "mod_primary"
                    ), diagnostic);
                }
            }
        }
    }

    private static async Task<Document> FixOptionDefinitionArgList(Document document, ArgumentListSyntax argList, CancellationToken cancellationToken)
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

    private static async Task<Document> FixPrimaryConstructor(Document document, ClassDeclarationSyntax decl, CancellationToken cancellationToken)
    {
        var id = SyntaxFactory.Identifier("init");
        var oldParams = decl.ParameterList!.Parameters.ToList();
        oldParams.RemoveRange(0, 2);
        oldParams.Insert(0, SyntaxFactory.Parameter(id).WithType(SyntaxFactory.ParseTypeName("ModuleInit")));

        if (decl.BaseList!.Types.Single() is not PrimaryConstructorBaseTypeSyntax oldBase)
            throw new InvalidOperationException("Internal error in code fix: FixPrimaryConstructor called on a declaration without a primary constructor");

        var oldBaseParams = oldBase.ArgumentList.Arguments.ToList();
        oldBaseParams.RemoveRange(0, 2);
        oldBaseParams.Insert(0, SyntaxFactory.Argument(SyntaxFactory.IdentifierName("init")));

        var newDecl = decl
            .WithParameterList(decl.ParameterList.WithParameters([.. oldParams]))
            .WithBaseList(SyntaxFactory.BaseList([oldBase.WithArgumentList(SyntaxFactory.ArgumentList([.. oldBaseParams]))]));

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = oldRoot!.ReplaceNode(decl, newDecl);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> FixConstructor(Document document, ConstructorDeclarationSyntax decl, CancellationToken cancellationToken)
    {
        var id = SyntaxFactory.Identifier("init");
        var oldParams = decl.ParameterList!.Parameters.ToList();
        oldParams.RemoveRange(0, 2);
        oldParams.Insert(0, SyntaxFactory.Parameter(id).WithType(SyntaxFactory.ParseTypeName("ModuleInit")));

        var oldBaseParams = decl.Initializer!.ArgumentList.Arguments.ToList();
        oldBaseParams.RemoveRange(0, 2);
        oldBaseParams.Insert(0, SyntaxFactory.Argument(SyntaxFactory.IdentifierName("init")));

        var newDecl = decl
            .WithParameterList(decl.ParameterList.WithParameters([.. oldParams]))
            .WithInitializer(decl.Initializer.WithArgumentList(SyntaxFactory.ArgumentList([.. oldBaseParams])));

        var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
        var newRoot = oldRoot!.ReplaceNode(decl, newDecl);
        return document.WithSyntaxRoot(newRoot);
    }
}
