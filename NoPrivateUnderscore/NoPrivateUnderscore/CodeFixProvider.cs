using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace NoPrivateUnderscore
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NoPrivateUnderscoreCodeFixProvider)), Shared]
	public class NoPrivateUnderscoreCodeFixProvider : CodeFixProvider
	{
		private const string title = "Remove underscore";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(NoPrivateUnderscoreAnalyzer.DiagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			// See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			// TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			// Find the type declaration identified by the diagnostic.
			var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(
				CodeAction.Create(
					title: title,
					createChangedSolution: c => MakeUppercaseAsync(context.Document, declaration, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Solution> MakeUppercaseAsync(Document document, FieldDeclarationSyntax typeDecl, CancellationToken cancellationToken)
		{
			var model = await document.GetSemanticModelAsync();

			foreach (var variable in typeDecl.Declaration.Variables)
			{
				var fieldSymbol = model.GetDeclaredSymbol(variable);
				// Do stuff with the symbol here

				// Compute new uppercase name.
				var identifierToken = fieldSymbol.Name;
				var newName = fieldSymbol.Name.Substring(1);

				// Produce a new solution that has all references to that type renamed, including the declaration.
				var originalSolution = document.Project.Solution;
				var optionSet = originalSolution.Workspace.Options;
				var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, fieldSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

				// Return the new solution with the now-uppercase type name.
				return newSolution;
			}

			return null;

			
		}
	}
}