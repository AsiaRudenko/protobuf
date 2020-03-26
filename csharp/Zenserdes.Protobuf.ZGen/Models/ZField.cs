using Google.Protobuf.Reflection;

using Humanizer;
using System;
using System.Text;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public class ZField
	{
		public ZField(string fieldName, int index, string cSharpType)
		{
			FieldName = fieldName;
			Index = index;
			CSharpType = cSharpType;
		}

		public string FieldName { get; }
		public int Index { get; }
		public string CSharpType { get; }
	}

	public static partial class Extensions
	{
		public static ZField From(this FieldDescriptorProto proto, string @namespace)
		{
			var fieldName = proto.Name.Pascalize();
			var index = proto.Number;
			var cSharpType = proto.ToSourceType(@namespace);

			return new ZField(fieldName, index, cSharpType);
		}

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
	}
}