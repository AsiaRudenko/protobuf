using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Text;

#nullable enable

namespace Zenserdes.Protobuf.ZGen
{
	internal class Program
	{
		/// <summary>
		/// ZGen - Generate protobuf files for Zenserdes.Protobuf
		/// </summary>
		/// <param name="inputProto">The input proto file.</param>
		/// <param name="outputDirectory">The output directory.</param>
		/// <param name="namespace">The namespace to use</param>
		private static void Main(string inputProto, string outputDirectory, string? @namespace = null)
		{
			// Google.Protobuf.Reflection.FileDescriptorSet
			// // Summary:
			// Default package to use when none is specified; can use #FILE# and #DIR# tokens
			// public string DefaultPackage { get; set; }
			var protoNamespace = @namespace ?? "#FILE#";

			var files = ParserHelpers.Parse(inputProto, protoNamespace);

			if (files == null)
			{
				return;
			}

			// fight me
			const string tabString = "\t";

			var stringBuilder = new StringBuilder();
			using var writer = new IndentedTextWriter(new StringWriter(stringBuilder), tabString);
			var generator = new ProtobufGenerator.Struct(writer, protoNamespace);

			foreach (var file in files)
			{
				generator.Generate(file);
			}

			Console.WriteLine(stringBuilder.ToString());
		}
	}
}
