using Zenserdes.Protobuf.Serialization;

#nullable enable

namespace Zenserdes.Protobuf
{
	// TODO: figure out what would be essential to have

	/// <summary>
	/// Represents a message that is capable of being serialized by Zenserdes.Protobuf.
	/// </summary>
	public interface IMessage
	{
		/// <summary>
		/// Hints at a probable size for the message. It isn't accurate in the slightest,
		/// it's just a good guess fro
		/// </summary>
		public int SizeHint { get; }

		// TODO: use more specific data types
		// for now, these serve as stubs
		bool Serialize(MemoryScriber target);

		bool Serialize(SpanScriber target);

		void Serialize(StreamScriber target);
	}
}