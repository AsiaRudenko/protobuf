using System;
using System.CodeDom.Compiler;

using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.ZenGen
{
	public static partial class ProtobufGenerator
	{
		public class TryDeserialize
		{
			private IndentedTextWriter _writer;

			public TryDeserialize(IndentedTextWriter writer) => _writer = writer;

			public void Generate(ZMessage message, string fullyQualifiedMessageName, Type dataStreamer)
			{
				_writer.WriteMethod(typeof(bool), typeof(IMessageOperator<>), new string[] { fullyQualifiedMessageName }, nameof(IMessageOperator<IWantToUseNameof>.TryDeserialize) + "<TBufferWriter>", () =>
				{
					_writer.WriteLine($"byte wireByte = default;");
					_writer.WriteLine();
					_writer.WriteLine($"while (dataStreamer.Next(ref wireByte))");
					_writer.WriteLine('{');
					_writer.Indent++;

					_writer.WriteLine("switch (wireByte)");
					_writer.WriteLine('{');
					_writer.Indent++;

					foreach (var (field, isLast) in message.Fields.FlagLast())
					{
						var fieldId = (byte)field.Index;
						var wireType = (byte)field.WireType;

						var wireByte = (fieldId << 3) | wireType;

						var binary = Convert.ToString(wireByte, 2).PadLeft(8, '0');
						var withUnderscore = binary.Substring(0, 5) + "_" + binary.Substring(5, 3);

						_writer.WriteLine($"case 0b{withUnderscore}:");
						_writer.WriteLine('{');
						_writer.Indent++;

						_writer.WriteLine("break;");

						_writer.Indent--;
						_writer.WriteLine('}');

						if (!isLast)
						{
							_writer.WriteLine();
						}
					}

					_writer.Indent--;
					_writer.WriteLine('}');

					_writer.Indent--;
					_writer.WriteLine('}');
					_writer.WriteLine();
					_writer.WriteLine("return true;");
				}, $"ref {dataStreamer.FullyQualified("TBufferWriter")} dataStreamer", $"ref {fullyQualifiedMessageName} instance");
			}
		}
	}
}