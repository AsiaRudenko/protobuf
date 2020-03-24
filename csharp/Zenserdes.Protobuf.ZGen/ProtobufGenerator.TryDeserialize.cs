using Google.Protobuf.Reflection;
using System;
using System.Buffers;
using System.CodeDom.Compiler;
using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.ZGen
{
	public static partial class ProtobufGenerator
	{
		public class TryDeserialize
		{
			private IndentedTextWriter _writer;

			public TryDeserialize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(DescriptorProto message, string fullyQualifiedMessageName, Type dataStreamer)
			{
				_writer.WriteMethod(typeof(bool), typeof(IMessageOperator<>), new string[] { fullyQualifiedMessageName }, nameof(IMessageOperator<IWantToUseNameof>.TryDeserialize) + "<TBufferWriter>", () =>
				{
					_writer.WriteLine("throw new " + typeof(NotImplementedException).FullyQualified() + "();");
				}, $"ref {dataStreamer.FullyQualified("TBufferWriter")} message", $"ref {fullyQualifiedMessageName} instance");
			}
		}
	}
}