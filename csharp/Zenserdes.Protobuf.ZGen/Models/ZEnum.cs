using Google.Protobuf.Reflection;

using System.Collections.Generic;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public class ZEnum
	{
		public ZEnum(string name, List<ZEnumValue> values, string fullName)
		{
			Name = name;
			Values = values;
			FullName = fullName;
		}

		public string Name { get; }
		public List<ZEnumValue> Values { get; }
		public string FullName { get; }
	}

	public static partial class Extensions
	{
		public static ZEnum From(this EnumDescriptorProto proto, string scope)
		{
			// can't use Pascalize because enums are SCREAMING_CASE, and when they're formatted
			// as "SUCH", Pascalize will return "SUCH". so, we tolower it so Pascalize will
			// return "Such".

			var name = proto.Name;

			var values = new List<ZEnumValue>();

			foreach (var value in proto.Values)
			{
				values.Add(value.From());
			}

			return new ZEnum(name, values, scope + name);
		}
	}
}