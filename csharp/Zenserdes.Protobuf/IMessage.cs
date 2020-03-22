#nullable enable

using System;
using System.Buffers;
using System.IO;

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
		Memory<byte> Serialize<TBufferWriter>(TBufferWriter bufferWriter)
			where TBufferWriter : IBufferWriter<byte>;

		bool Serialize(Span<byte> target);

		void Serialize(Stream target);
	}
}