using Google.Protobuf.Reflection;

using System.Collections.Generic;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public class ZProtobufFile
	{
		public ZProtobufFile(List<ZMessage> messages, List<ZEnum> enums, string fullName)
		{
			Messages = messages;
			Enums = enums;
			FullName = fullName;
		}

		public List<ZMessage> Messages { get; }
		public List<ZEnum> Enums { get; }
		public string FullName { get; }
	}

	public static partial class Extensions
	{
		// `@namespace` is: `Your.Cool.Namespace`
		// `scope` should be: `global::Your.Cool.Namespace`
		public static ZProtobufFile From(this FileDescriptorProto proto, string @namespace, string scope)
		{
			var messages = new List<ZMessage>();

			foreach (var message in proto.MessageTypes)
			{
				messages.Add(message.From(@namespace, scope));
			}

			var enums = new List<ZEnum>();

			foreach (var @enum in proto.EnumTypes)
			{
				enums.Add(@enum.From(scope));
			}

			return new ZProtobufFile(messages, enums, scope);
		}
	}
}