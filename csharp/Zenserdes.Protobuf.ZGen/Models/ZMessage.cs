using Google.Protobuf.Reflection;

using System.Collections.Generic;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public class ZMessage
	{
		public ZMessage(List<ZField> fields, List<ZMessage> nestedMessages, List<ZEnum> nestedEnums, string fullName, string name)
		{
			Fields = fields;
			NestedMessages = nestedMessages;
			NestedEnums = nestedEnums;
			FullName = fullName;
			Name = name;
		}

		public List<ZField> Fields { get; }
		public List<ZMessage> NestedMessages { get; }
		public List<ZEnum> NestedEnums { get; }
		public string FullName { get; }
		public string Name { get; }
	}

	public static partial class Extensions
	{
		public static ZMessage From(this DescriptorProto proto, string @namespace, string scope)
		{
			var fields = new List<ZField>();

			foreach (var field in proto.Fields)
			{
				fields.Add(field.From(@namespace));
			}

			var innerScope = scope + "Types.";

			var nestedMessages = new List<ZMessage>();

			foreach (var nestedMessage in proto.NestedTypes)
			{
				nestedMessages.Add(nestedMessage.From(@namespace, innerScope));
			}

			var nestedEnums = new List<ZEnum>();

			foreach (var nestedEnum in proto.EnumTypes)
			{
				nestedEnums.Add(nestedEnum.From(innerScope));
			}

			return new ZMessage(fields, nestedMessages, nestedEnums, scope + proto.Name, proto.Name);
		}
	}
}