using System.CodeDom.Compiler;

namespace Zenserdes.Protobuf.ZenGen
{
	public ref struct CodeBlock
	{
		private readonly IndentedTextWriter _writer;

		public CodeBlock(IndentedTextWriter writer)
		{
			_writer = writer;
			_writer.WriteLine('{');
			_writer.Indent++;
		}

		public void Dispose()
		{
			_writer.Indent--;
			_writer.WriteLine('}');
		}
	}

	public static class CodeBlockExtensions
	{
		public static CodeBlock WithCodeBlock(this IndentedTextWriter writer)
			=> new CodeBlock(writer);
	}
}