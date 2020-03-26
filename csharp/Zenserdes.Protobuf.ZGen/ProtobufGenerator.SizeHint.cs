using System.CodeDom.Compiler;

using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.ZenGen
{
	public static partial class ProtobufGenerator
	{
		public class SizeHint
		{
			private IndentedTextWriter _writer;

			public SizeHint(IndentedTextWriter writer) => _writer = writer;

			public void Generate(ZMessage message)
			{
				// TODO: calculate a good size hint
				_writer.WriteLine("public int SizeHint => 256; // TODO: calculate a good size hint");
			}
		}
	}
}