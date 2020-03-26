using Google.Protobuf.Reflection;

using System;
using System.Collections.Generic;
using System.Text;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public enum WireType
	{
		Varint = 0b00000_000,
		Bit64 = 0b00000_001,
		LengthDelimited = 0b00000_010,
		[Obsolete] StartGroup = 0b00000_011,
		[Obsolete] EndGroup = 0b00000_100,
		Bit32 = 0b00000_101,
	};

	public enum SerializationImplementation
	{
		Varint32,
		Zigzag32,
		Fixed32,
		Varint64,
		Zigzag64,
		Fixed64,
		Bytes,
	}

	public static class CSharpInteropt
	{
		public static string ToSourceType(this FieldDescriptorProto proto, string @namespace)
		{
			var cSharpType = proto.ToCSharpType();

			if (cSharpType != typeof(object))
			{
				return cSharpType.ToSourceType();
			}

			// in the format `.Nested.Messages`
			var typeName = proto.TypeName;

			var builder = new StringBuilder("global::");
			builder.Append(@namespace);

			typeName = typeName.Substring(1);
			builder.Append('.');

			typeName = typeName.Replace(".", ".Types.");
			builder.Append(typeName);

			return builder.ToString();
		}

		public static Type ToCSharpType(this FieldDescriptorProto proto)
		{
			switch (proto.type)
			{
				case FieldDescriptorProto.Type.TypeBool: return typeof(bool);

				case FieldDescriptorProto.Type.TypeInt32:
				case FieldDescriptorProto.Type.TypeSint32:
				case FieldDescriptorProto.Type.TypeFixed32:
				case FieldDescriptorProto.Type.TypeSfixed32:
					return typeof(int);

				case FieldDescriptorProto.Type.TypeUint32: return typeof(uint);

				case FieldDescriptorProto.Type.TypeInt64:
				case FieldDescriptorProto.Type.TypeSint64:
				case FieldDescriptorProto.Type.TypeFixed64:
				case FieldDescriptorProto.Type.TypeSfixed64:
					return typeof(long);

				case FieldDescriptorProto.Type.TypeUint64: return typeof(ulong);

				case FieldDescriptorProto.Type.TypeFloat: return typeof(float);
				case FieldDescriptorProto.Type.TypeDouble: return typeof(double);

				case FieldDescriptorProto.Type.TypeString:
				case FieldDescriptorProto.Type.TypeBytes:
					return typeof(ReadOnlyMemory<byte>);

				case FieldDescriptorProto.Type.TypeMessage:
				case FieldDescriptorProto.Type.TypeEnum:
				default: return typeof(object);

				case FieldDescriptorProto.Type.TypeGroup:
					throw new NotSupportedException("Deprecated features are not going to be implemented.");
			}
		}

		private static readonly Dictionary<Type, string> _shortcuts = new Dictionary<Type, string>
		{
			[typeof(sbyte)] = "sbyte",
			[typeof(byte)] = "byte",
			[typeof(short)] = "short",
			[typeof(ushort)] = "ushort",
			[typeof(int)] = "int",
			[typeof(uint)] = "uint",
			[typeof(long)] = "long",
			[typeof(ulong)] = "ulong",
			[typeof(bool)] = "bool",
			[typeof(double)] = "double",
			[typeof(string)] = "string",
			[typeof(object)] = "object",
			[typeof(void)] = "void",
		};

		// 'source' as in 'source code type'
		public static string ToSourceType(this Type type, string[]? genericArgReplacements = null)
		{
			genericArgReplacements ??= Array.Empty<string>();

			if (_shortcuts.TryGetValue(type, out var shortcut))
			{
				return shortcut;
			}

			var nameBuilder = new StringBuilder("global::");

			if (type.Namespace != null)
			{
				nameBuilder.Append(type.Namespace);
				nameBuilder.Append('.');
			}

			ReadOnlySpan<char> name = type.Name;
			var backtickIndex = name.IndexOf('`');

			if (backtickIndex >= 0)
			{
				name = name.Slice(0, backtickIndex);
			}

			nameBuilder.Append(name);

			if (type.IsGenericType)
			{
				nameBuilder.Append('<');

				var i = 0;
				foreach (var (generic, isLast) in type.GetGenericArguments().FlagLast())
				{
					if (genericArgReplacements.Length > i)
					{
						nameBuilder.Append(genericArgReplacements[i]);
					}
					else
					{
						nameBuilder.Append(generic.ToSourceType());
					}

					if (!isLast)
					{
						nameBuilder.Append(", ");
					}
					i++;
				}

				nameBuilder.Append('>');
			}

			return nameBuilder.ToString();
		}

		public static WireType ToWireType(this FieldDescriptorProto proto)
		{
			switch (proto.type)
			{
				case FieldDescriptorProto.Type.TypeBool:

				case FieldDescriptorProto.Type.TypeEnum:

				case FieldDescriptorProto.Type.TypeInt32:
				case FieldDescriptorProto.Type.TypeUint32:
				case FieldDescriptorProto.Type.TypeSint32:

				case FieldDescriptorProto.Type.TypeInt64:
				case FieldDescriptorProto.Type.TypeUint64:
				case FieldDescriptorProto.Type.TypeSint64:
					return WireType.Varint;

				case FieldDescriptorProto.Type.TypeString:
				case FieldDescriptorProto.Type.TypeBytes:
				case FieldDescriptorProto.Type.TypeMessage:
					return WireType.LengthDelimited;

				case FieldDescriptorProto.Type.TypeFloat:
				case FieldDescriptorProto.Type.TypeFixed32:
					return WireType.Bit32;

				case FieldDescriptorProto.Type.TypeDouble:
				case FieldDescriptorProto.Type.TypeFixed64:
					return WireType.Bit64;

				default: throw new NotSupportedException("Deprecated features not supported.");
			}
		}

		public static SerializationImplementation ToSerializationImplementation(this FieldDescriptorProto proto)
		{
			switch (proto.type)
			{
				case FieldDescriptorProto.Type.TypeBool:
				case FieldDescriptorProto.Type.TypeEnum:
				case FieldDescriptorProto.Type.TypeInt32:
				case FieldDescriptorProto.Type.TypeUint32:
					return SerializationImplementation.Varint32;

				case FieldDescriptorProto.Type.TypeSint32:
					return SerializationImplementation.Zigzag32;

				case FieldDescriptorProto.Type.TypeInt64:
				case FieldDescriptorProto.Type.TypeUint64:
					return SerializationImplementation.Varint64;

				case FieldDescriptorProto.Type.TypeSint64:
					return SerializationImplementation.Zigzag64;

				case FieldDescriptorProto.Type.TypeString:
				case FieldDescriptorProto.Type.TypeBytes:
				case FieldDescriptorProto.Type.TypeMessage:
					return SerializationImplementation.Bytes;

				case FieldDescriptorProto.Type.TypeFloat:
				case FieldDescriptorProto.Type.TypeFixed32:
					return SerializationImplementation.Fixed32;

				case FieldDescriptorProto.Type.TypeDouble:
				case FieldDescriptorProto.Type.TypeFixed64:
					return SerializationImplementation.Fixed64;

				default: throw new NotSupportedException("Deprecated features not supported.");
			}
		}
	}
}