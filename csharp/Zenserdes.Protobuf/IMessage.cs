#nullable enable

using System;
using System.Buffers;
using System.IO;

using Zenserdes.Protobuf.Serialization;

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

	/// <summary>
	/// Provides static methods on a <see cref="IMessage"/>, in place
	/// of static methods on a class of some sort. This is to circumvent the inability
	/// for interfaces to define generic static methods. When this is used, it is combined
	/// with the struct constraint in order to ensure that all method calls are free.
	/// </summary>
	/// <typeparam name="TMessage">The type of message this operator operates on.</typeparam>
	public interface IMessageOperator<TMessage>
		where TMessage : IMessage
	{
		ulong ExactSize(in TMessage message);

		ulong ExactSize(TMessage message);

		bool TryDeserialize(ref MemoryDataStreamer dataStreamer, ref TMessage instance);

		bool TryDeserialize<TBufferWriter>(ref SpanDataStreamer<TBufferWriter> dataStreamer, ref TMessage instance)
			where TBufferWriter : IBufferWriter<byte>;

		bool TryDeserialize<TBufferWriter>(ref StreamDataStreamer<TBufferWriter> dataStreamer, ref TMessage instance)
			where TBufferWriter : IBufferWriter<byte>;
	}

	public interface IMessageAndOperator<TSelf> : IMessage, IMessageOperator<TSelf>
		where TSelf : struct, IMessageAndOperator<TSelf>
	{
	}
}