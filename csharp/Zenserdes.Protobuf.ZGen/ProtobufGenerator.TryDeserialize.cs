using System;
using System.Buffers;
using System.CodeDom.Compiler;
using Zenserdes.Protobuf.Serialization;
using Zenserdes.Protobuf.ZenGen.Models;

namespace Zenserdes.Protobuf.ZenGen
{
	public static partial class ProtobufGenerator
	{
		public class TryDeserialize
		{
			private IndentedTextWriter _writer;

			public TryDeserialize(IndentedTextWriter writer) => _writer = writer;

			// hungarian notation: `n` == `name`
			private const string n_wireByte = "wireByte";
			private const string n_temp = "temp";
			private const string n_dataStreamer = "dataStreamer";
			private const string n_instance = "instance";

			// hunagrian notation: `m` == `method`
			// TODO: remove
			private const string m_MaybeRead = nameof(MemoryDataStreamer<IBufferWriter<byte>>.MaybeRead);

			public void Generate(ZMessage message, string fullyQualifiedMessageName, Type dataStreamer)
			{
				_writer.WriteMethod(typeof(bool), typeof(IMessageOperator<>), new string[] { fullyQualifiedMessageName }, nameof(IMessageOperator<IWantToUseNameof>.TryDeserialize) + "<TBufferWriter>", () =>
				{
					_writer.WriteLine($"{typeof(byte).FullyQualified()} {n_wireByte} = default;");
					_writer.WriteLine($"{typeof(Span<byte>).FullyQualified()} {n_temp} = stackalloc byte[10];");
					_writer.WriteLine();
					_writer.WriteLine($"while ({n_dataStreamer}.Next(ref {n_wireByte}))");
					_writer.WriteLine('{');
					_writer.Indent++;

					_writer.WriteLine($"switch ({n_wireByte})");
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

						// TODO: use lookup table
						switch (field.SerializationImplementation)
						{
							case SerializationImplementation.Varint32:
							{
								GenerateSerializerCodeVarint(_writer, field);
							}
							break;

							case SerializationImplementation.Bytes:
							{
								GenerateSerializerCodeBytes(_writer, field);
							}
							break;

							default:
							{
								_writer.WriteLine("// TODO");
							}
							break;
						}

						_writer.Indent--;
						_writer.WriteLine('}');
						_writer.WriteLine("break;");

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
				}, $"ref {dataStreamer.FullyQualified("TBufferWriter")} {n_dataStreamer}", $"ref {fullyQualifiedMessageName} {n_instance}");
			}

			public static void GenerateSerializerCodeVarint(IndentedTextWriter writer, ZField field)
			{
				writer.WriteLine($"{n_dataStreamer}.{nameof(MemoryDataStreamer<IBufferWriter<byte>>.MaybeReadWorkaround)}(10).CopyTo({n_temp});");
				writer.WriteLine();
				writer.WriteLine("var offset = 0;");
				writer.WriteLine("var result = 0u;");
				writer.WriteLine($"if (!{typeof(DataDecoder).FullyQualified()}.{nameof(DataDecoder.TryReadVarint32)}({n_temp}, ref offset, ref result))");

				using (var _ = writer.WithCodeBlock())
				{
					writer.WriteLine("return false;");
				}

				writer.WriteLine();
				writer.WriteLine($"{n_dataStreamer}.{nameof(MemoryDataStreamer<IBufferWriter<byte>>.Advance)}(offset);");
				writer.WriteLine($"{n_instance}.{field.FieldName} = ({field.CSharpType})result;");
			}

			public static void GenerateSerializerCodeBytes(IndentedTextWriter writer, ZField field)
			{
				// varint code

				writer.WriteLine($"{n_dataStreamer}.{nameof(MemoryDataStreamer<IBufferWriter<byte>>.MaybeReadWorkaround)}(10).CopyTo({n_temp});");
				writer.WriteLine();
				writer.WriteLine("var offset = 0;");
				writer.WriteLine("var result = 0u;");
				writer.WriteLine($"if (!{typeof(DataDecoder).FullyQualified()}.{nameof(DataDecoder.TryReadVarint32)}({n_temp}, ref offset, ref result))");

				using (var _ = writer.WithCodeBlock())
				{
					writer.WriteLine("return false;");
				}

				writer.WriteLine();
				writer.WriteLine($"{n_dataStreamer}.{nameof(MemoryDataStreamer<IBufferWriter<byte>>.Advance)}(offset);");

				writer.WriteLine($"if (!{n_dataStreamer}.{nameof(MemoryDataStreamer<IBufferWriter<byte>>.ReadPermanent)}((int)result, out var memory))");

				using (var _ = writer.WithCodeBlock())
				{
					writer.WriteLine("return false;");
				}

				writer.WriteLine();
				writer.Write($"{n_instance}.{field.FieldName} = memory;");
			}
		}
	}
}