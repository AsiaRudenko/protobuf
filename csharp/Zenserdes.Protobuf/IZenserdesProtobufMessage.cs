#nullable enable

namespace Zenserdes.Protobuf
{
	/// <summary>
	/// Represents a message that is capable of being serialized by Zenserdes.Protobuf.
	/// </summary>
	public interface IZenserdesProtobufMessage
	{
		/// <summary>
		/// Hints at the size of the message, typically used for as the initial
		/// capacity of some buffer.
		/// </summary>
		int SizeHint { get; }
	}
}
