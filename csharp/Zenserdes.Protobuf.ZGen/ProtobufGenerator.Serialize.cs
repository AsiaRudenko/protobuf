using Google.Protobuf.Reflection;
using System;
using System.CodeDom.Compiler;
using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.ZGen
{
	public static partial class ProtobufGenerator
	{
		public class Serialize
		{
			private IndentedTextWriter _writer;

			public Serialize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(DescriptorProto message, Type type)
			{
				var returnType = type == typeof(StreamScriber) ? typeof(void) : typeof(bool);
				_writer.WriteMethod(returnType, typeof(IMessage), null, nameof(IMessage.Serialize), () =>
				{
					_writer.WriteLine("throw new " + typeof(NotImplementedException).FullyQualified() + "();");
				}, $"{type.FullyQualified()} target");
			}
		}
	}
}