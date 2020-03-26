using Google.Protobuf.Reflection;

using Humanizer;

namespace Zenserdes.Protobuf.ZenGen.Models
{
	public struct ZFieldOptions
	{
		public bool Deprecated { get; set; }
	}

	public class ZField
	{
		public ZField(string fieldName, int index, string cSharpType, ZFieldOptions options, WireType wireType, SerializationImplementation serializationImplementation)
		{
			FieldName = fieldName;
			Index = index;
			CSharpType = cSharpType;
			Options = options;
			WireType = wireType;
			SerializationImplementation = serializationImplementation;
		}

		public string FieldName { get; }
		public int Index { get; }
		public ZFieldOptions Options { get; }

		// implementation stuff i guess
		public string CSharpType { get; }

		public WireType WireType { get; }
		public SerializationImplementation SerializationImplementation { get; }
	}

	public static partial class Extensions
	{
		public static ZField From(this FieldDescriptorProto proto, string @namespace)
		{
			var fieldName = proto.Name.Pascalize();
			var index = proto.Number;
			var cSharpType = proto.ToSourceType(@namespace);

			var options = new ZFieldOptions
			{
				Deprecated = proto.Options?.Deprecated == true ? true : false
			};

			var wireType = proto.ToWireType();
			var serializationImplementation = proto.ToSerializationImplementation();

			return new ZField(fieldName, index, cSharpType, options, wireType, serializationImplementation);
		}
	}
}