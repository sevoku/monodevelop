using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Web.Razor;
using System.Web.Razor.Generator;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Collections.Generic;

namespace MonoDevelop.RazorGenerator
{
	class PreprocessedRazorHost : RazorEngineHost
	{
		static readonly IEnumerable<string> defaultImports = new[] {
			"System",
			"System.Collections.Generic",
			"System.Linq",
			"System.Text"
		};
		readonly CodeDomProvider _codeDomProvider;
		readonly CodeGeneratorOptions codeGeneratorOptions;
		string defaultClassName;

		public PreprocessedRazorHost (string fullPath) : base (RazorCodeLanguage.GetLanguageByExtension (".cshtml"))
		{
			if (fullPath == null)
				throw new ArgumentNullException ("fullPath");

			FullPath = fullPath;
			_codeDomProvider = new Microsoft.CSharp.CSharpCodeProvider ();
			DefaultNamespace = "ASP";
			EnableLinePragmas = true;

			GeneratedClassContext = new GeneratedClassContext (
				GeneratedClassContext.DefaultExecuteMethodName,
				GeneratedClassContext.DefaultWriteMethodName,
				GeneratedClassContext.DefaultWriteLiteralMethodName,
				"WriteTo",
				"WriteLiteralTo",
				"Action<System.IO.TextWriter>",
				"DefineSection",
				"BeginContext",
				"EndContext"
			) {
				ResolveUrlMethodName = "Href"
			};

			codeGeneratorOptions = new CodeGeneratorOptions {
				// HACK: we use true, even though razor uses false, to work around a mono bug where it omits the 
				// line ending after "#line hidden", resulting in the unparseable "#line hiddenpublic"
				BlankLinesBetweenMembers = true,
				BracingStyle = "C",
				// matches Razor built-in settings
				IndentString = String.Empty,
			};

			foreach (var import in defaultImports)
				NamespaceImports.Add (import);
		}

		public string FullPath {
			get; private set;
		}

		public override string DefaultClassName {
			get {
				return defaultClassName ?? GetClassName ();
			}
			set {
				if (!string.Equals (value, "__CompiledTemplate", StringComparison.OrdinalIgnoreCase)) {
					//  By default RazorEngineHost assigns the name __CompiledTemplate. We'll ignore this assignment
					defaultClassName = value;
				}
			}
		}

		public bool EnableLinePragmas { get; set; }

		public string GenerateCode (out CompilerErrorCollection errors)
		{
			errors = new CompilerErrorCollection ();

			var engine = new RewritingRazorTemplateEngine (this, new PreprocessedAttributeRewriter ());

			// Generate code
			GeneratorResults results;
			try {
				Stream stream = File.OpenRead (FullPath);
				using (var reader = new StreamReader (stream, Encoding.Default, true)) {
					results = engine.GenerateCode (reader, DefaultClassName, DefaultNamespace, FullPath);
				}
			} catch (Exception e) {
				errors.Add (new CompilerError (FullPath, 1, 1, null, e.ToString ()));
				//Returning null signifies that generation has failed
				return null;
			}

			// Output errors
			foreach (RazorError error in results.ParserErrors) {
				errors.Add (new CompilerError (FullPath, error.Location.LineIndex + 1, error.Location.CharacterIndex + 1, null, error.Message));
			}

			try {
				using (var writer = new StringWriter ()) {
					writer.WriteLine ("#pragma warning disable 1591");
					_codeDomProvider.GenerateCodeFromCompileUnit (results.GeneratedCode, writer, codeGeneratorOptions);
					writer.WriteLine ("#pragma warning restore 1591");
					return writer.ToString ();
				}
			} catch (Exception e) {
				errors.Add (new CompilerError (FullPath, 1, 1, null, e.ToString ()));
				//Returning null signifies that generation has failed
				return null;
			}
		}

		public override void PostProcessGeneratedCode (CodeGeneratorContext context)
		{
			PreprocessedTemplateCodeTransformers.AddGeneratedTemplateClassAttribute (context.GeneratedClass);
			PreprocessedTemplateCodeTransformers.InjectBaseClass (context.Namespace, context.GeneratedClass, context.TargetMethod);
			PreprocessedTemplateCodeTransformers.MakePartialAndRemoveCtor (context.GeneratedClass);
		}

		public override RazorCodeGenerator DecorateCodeGenerator (RazorCodeGenerator incomingCodeGenerator)
		{
			incomingCodeGenerator = new PreprocessedCSharpRazorCodeGenerator (incomingCodeGenerator);
			var codeGenerator = base.DecorateCodeGenerator (incomingCodeGenerator);
			codeGenerator.GenerateLinePragmas = EnableLinePragmas;
			return codeGenerator;
		}

		public override ParserBase DecorateCodeParser (ParserBase incomingCodeParser)
		{
			return new PreprocessedCSharpRazorCodeParser ();
		}

		protected virtual string GetClassName ()
		{
			string filename = Path.GetFileNameWithoutExtension (FullPath);
			return ParserHelpers.SanitizeClassName (filename);
		}
	}
}