using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Xunit.Abstractions;

using Zenserdes.Protobuf.ZenGen;
using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.Tests.GeneratedCodeTests
{
	public abstract class CompilationTest
	{
		protected readonly ITestOutputHelper _logger;

		protected CompilationTest(ITestOutputHelper logger)
		{
			_logger = logger;
		}

		protected Task RunCode(string protobufCode, string testCode, string? @namespace = null)
			=> TestcaseCompiler.RunCode(_logger, protobufCode, testCode, @namespace);
	}

	public static class TestcaseCompiler
	{
		public static Task RunCode(string protobufCode, string cSharpScript, string? @namespace = null)
			=> RunCode(null, protobufCode, cSharpScript, @namespace);

		// TODO: cleanup stuff
		public static Task RunCode(ITestOutputHelper? logger, string protobufFile, string csharpScript, string? @namespace = null)
		{
			// parse protobuf code
			@namespace ??= string.Empty;
			var protoDescriptor = ParserHelpers.ParseText(protobufFile, @namespace);

			if (protoDescriptor == null)
			{
				throw new InvalidOperationException("Unable to parse protobuf file.");
			}

			var scope = "global::" + @namespace;

			if (@namespace.Length > 0)
			{
				scope += ".";
			}

			var model = protoDescriptor.From(@namespace, scope);

			var stringBuilder = new StringBuilder();
			using var writer = new IndentedTextWriter(new StringWriter(stringBuilder), "\t");
			var generator = new ProtobufGenerator.Struct(writer, @namespace);

			generator.Generate(model);

			// write the roslyn scripting code
			var roslynScript = new StringBuilder();
			roslynScript.AppendLine();
			roslynScript.AppendLine("public void TestCode()");
			roslynScript.AppendLine("{");

			foreach (var line in csharpScript.Split('\n'))
			{
				roslynScript.AppendLine("\t" + line.Trim());
			}

			roslynScript.AppendLine("}");
			roslynScript.AppendLine();
			roslynScript.AppendLine("TestCode()");

			var generatedCode = stringBuilder.ToString();
			logger?.WriteLine("{0}", generatedCode);

			var roslynCode = roslynScript.ToString();
			logger?.WriteLine("{0}", roslynCode);

			return CSharpLanguageService.Evaluate(generatedCode, roslynCode, @namespace);
		}
	}

	public static class CSharpLanguageService
	{
		private const string _generatedAssemblyName = "GeneratedZenserdesProtobufCode";

		private static MetadataReference[] _assemblyReferences = new MetadataReference[]
		{
			// testing libraries
			MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(FluentAssertions.AssertionExtensions).Assembly.Location),

			// zenserdes
			MetadataReference.CreateFromFile(typeof(ZenserdesProtobuf).Assembly.Location),
		};

		private static ScriptOptions _scriptOptions = ScriptOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.AddReferences(_assemblyReferences)
			.WithImports("System", "Zenserdes.Protobuf", "FluentAssertions");

		public static async Task Evaluate(string generatedCode, string scriptCode, string? @namespace = null)
		{
			var compilation = Compile(generatedCode);

			var scriptOptions = _scriptOptions
				.AddReferences(compilation.ToMetadataReference());

			if (@namespace?.Length > 0)
			{
				scriptOptions = scriptOptions.AddImports(@namespace);
			}

			await CSharpScript.RunAsync(scriptCode, scriptOptions);
		}

		// https://josephwoodward.co.uk/2016/12/in-memory-c-sharp-compilation-using-roslyn
		private static CSharpCompilationOptions _compilationOptions = new CSharpCompilationOptions
		(
			outputKind: OutputKind.DynamicallyLinkedLibrary,
			optimizationLevel: OptimizationLevel.Debug,
			allowUnsafe: true
		);

		private static CSharpParseOptions _parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None);

		private static Compilation Compile(string selfCode)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(selfCode, _parseOptions);

			var compilation = CSharpCompilation.Create
			(
				_generatedAssemblyName,
				syntaxTrees: new [] { syntaxTree },
				options: _compilationOptions,
				references: _assemblyReferences
			);

			return compilation;
		}
	}
}