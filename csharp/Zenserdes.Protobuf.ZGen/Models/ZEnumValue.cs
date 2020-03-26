using Google.Protobuf.Reflection;

namespace Zenserdes.Protobuf.ZGen.Models
{
	public class ZEnumValue
	{
		public ZEnumValue(string name, int index)
		{
			Name = name;
			Index = index;
		}

		public string Name { get; }
		public int Index { get; }
	}

	public static partial class Extensions
	{
		public static ZEnumValue From(this EnumValueDescriptorProto proto)
		{
			var name = proto.Name;
			var index = proto.Number;

			return new ZEnumValue(name, index);
		}
	}
}