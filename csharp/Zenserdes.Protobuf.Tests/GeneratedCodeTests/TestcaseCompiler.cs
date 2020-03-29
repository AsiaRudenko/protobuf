using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using System;
using System.Buffers;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
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

			return CSharpLanguageService.Evaluate(logger, generatedCode, roslynCode, @namespace);
		}
	}

	public static class CSharpLanguageService
	{
		private const string _generatedAssemblyName = "GeneratedZenserdesProtobufCode";

		private static Assembly LoadAssemblyByString(string fullName)
			=> AppDomain.CurrentDomain.GetAssemblies().First(x => x.FullName == fullName);

		private static MetadataReference[] _assemblyReferences = new MetadataReference[]
		{
			// testing libraries
			MetadataReference.CreateFromFile(typeof(Xunit.FactAttribute).Assembly.Location),
			MetadataReference.CreateFromFile(typeof(FluentAssertions.AssertionExtensions).Assembly.Location),

			// zenserdes
			MetadataReference.CreateFromFile(typeof(Zenserdes.Protobuf.ZenserdesProtobuf).Assembly.Location),

			// c#
			MetadataReference.CreateFromFile(typeof(System.Buffers.IBufferWriter<>).Assembly.Location),
			MetadataReference.CreateFromFile(LoadAssemblyByString("netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51").Location),
			MetadataReference.CreateFromFile(LoadAssemblyByString("System.Runtime, Version=4.2.2.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a").Location),
			MetadataReference.CreateFromFile(LoadAssemblyByString("System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e").Location),
		};

		private static ScriptOptions _scriptOptions = ScriptOptions.Default
			.WithLanguageVersion(LanguageVersion.CSharp8)
			.AddReferences(_assemblyReferences)
			.WithImports("System", "Zenserdes.Protobuf", "FluentAssertions");

		public static async Task Evaluate(ITestOutputHelper? logger, string generatedCode, string scriptCode, string? @namespace = null)
		{
			var compilation = Compile(logger, generatedCode);

			// compile to a file to satisfy script needing a file, (i guess?)
			var tempFile = Path.GetTempFileName();

			try
			{
				if (!compilation.Emit(tempFile).Success)
				{
					throw new CompilationErrorException("Couldn't emit assembly.", default);
				}

				var compilationReference = MetadataReference.CreateFromFile(tempFile);

				var scriptOptions = _scriptOptions
					.AddReferences(compilationReference);

				if (@namespace?.Length > 0)
				{
					scriptOptions = scriptOptions.AddImports(@namespace);
				}

				await CSharpScript.RunAsync(scriptCode, scriptOptions);
			}
			finally
			{
				// File.Delete(tempFile);
			}
		}

		// https://josephwoodward.co.uk/2016/12/in-memory-c-sharp-compilation-using-roslyn
		private static CSharpCompilationOptions _compilationOptions = new CSharpCompilationOptions
		(
			outputKind: OutputKind.DynamicallyLinkedLibrary,
			optimizationLevel: OptimizationLevel.Debug,
			allowUnsafe: true
		);

		private static CSharpParseOptions _parseOptions = new CSharpParseOptions(LanguageVersion.Latest, DocumentationMode.None);

		private static Compilation Compile(ITestOutputHelper? logger, string selfCode)
		{
			var syntaxTree = CSharpSyntaxTree.ParseText(selfCode, _parseOptions);

			var compilation = CSharpCompilation.Create
			(
				_generatedAssemblyName,
				syntaxTrees: new [] { syntaxTree },
				options: _compilationOptions,
				references: _assemblyReferences
			);

			var diagnostics = compilation.GetDiagnostics();

			if (diagnostics.Any(x => x.Severity == DiagnosticSeverity.Error))
			{
				foreach (var diagnostic in diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
				{
					logger?.WriteLine("{0}", diagnostic);
				}

				throw new CompilationErrorException("Failed to compile supplemental source code", diagnostics);
			}

			return compilation;
		}
	}
}