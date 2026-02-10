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
public class FixModuleInitProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [Analyzer.RuleUseModuleInitializer.Id];

    public override FixAllProvider? GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
        if (root == null)
            return;

        var diagnostic = context.Diagnostics.First();

        var decl = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent!;

        if (decl.FirstAncestorOrSelf<ConstructorDeclarationSyntax>() is { } cctor)
        {
            context.RegisterCodeFix(CodeAction.Create(
                title: "Use module initializer",
                createChangedDocument: c => FixConstructor(context.Document, cctor, c),
                equivalenceKey: "mod_ctor"
            ), diagnostic);
            return;
        }

        if (decl.FirstAncestorOrSelf<ClassDeclarationSyntax>() is { } cdecl)
        {
            context.RegisterCodeFix(CodeAction.Create(
                title: "Use module initializer",
                createChangedDocument: c => FixPrimaryConstructor(context.Document, cdecl, c),
                equivalenceKey: "mod_primary"
            ), diagnostic);
            return;
        }
    }

    private static async Task<Document> FixPrimaryConstructor(Document document, ClassDeclarationSyntax decl, CancellationToken cancellationToken)
    {
        var id = SyntaxFactory.Identifier("init");
        var oldParams = decl.ParameterList!.Parameters.ToList();
        oldParams.RemoveRange(0, 2);
        oldParams.Insert(0, SyntaxFactory.Parameter(id).WithType(SyntaxFactory.ParseTypeName("ModuleInitializer")));

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
        oldParams.Insert(0, SyntaxFactory.Parameter(id).WithType(SyntaxFactory.ParseTypeName("ModuleInitializer")));

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
