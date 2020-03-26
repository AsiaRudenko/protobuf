using System;
using System.CodeDom.Compiler;

namespace Zenserdes.Protobuf.ZGen
{
	public static class MethodWriterExtensions
	{
		// TODO: use builder pattern or something to clean up this lololo
		public static void WriteMethod(this IndentedTextWriter writer, Type returnType, Type interfaceType, string[]? genericArgsReplacements, string methodName, Action body, params string[] arguments)
			=> WriteMethod(writer, returnType, interfaceType, genericArgsReplacements, methodName, null, body, arguments);

		public static void WriteMethod(this IndentedTextWriter writer, Type returnType, Type interfaceType, string[]? genericArgsReplacements, string methodName, string? constraints, Action body, params string[] arguments)
		{
			// when implementing methods as an interface, you can't do this
			// writer.Write("public ");
			writer.Write(returnType.FullyQualified());
			writer.Write(' ');
			writer.Write(interfaceType.FullyQualified(genericArgsReplacements ?? Array.Empty<string>()));
			writer.Write('.');
			writer.Write(methodName);
			writer.Write('(');

			foreach (var (argument, isLast) in arguments.FlagLast())
			{
				writer.Write(argument);

				if (!isLast)
				{
					writer.Write(", ");
				}
			}

			writer.WriteLine(')');

			if (constraints != null)
			{
				writer.Indent++;
				writer.WriteLine(constraints);
				writer.Indent--;
			}

			writer.WriteLine('{');
			writer.Indent++;

			body();

			writer.Indent--;
			writer.WriteLine('}');
		}
	}
}