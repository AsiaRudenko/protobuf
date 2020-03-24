using Google.Protobuf.Reflection;
using System;
using System.CodeDom.Compiler;

namespace Zenserdes.Protobuf.ZGen
{
	public static partial class ProtobufGenerator
	{
		public class SizeHint
		{
			private IndentedTextWriter _writer;

			public SizeHint(IndentedTextWriter writer) => _writer = writer;

			public void Generate(DescriptorProto message)
			{
				_writer.WriteLine("public int SizeHint => 1;");
			}
		}
	}
}