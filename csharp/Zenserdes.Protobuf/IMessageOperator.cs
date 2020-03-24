#nullable enable

using System.Buffers;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf
{
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

		bool TryDeserialize<TBufferWriter>(ref MemoryDataStreamer<TBufferWriter> dataStreamer, ref TMessage instance)
			where TBufferWriter : IBufferWriter<byte>;

		bool TryDeserialize<TBufferWriter>(ref SpanDataStreamer<TBufferWriter> dataStreamer, ref TMessage instance)
			where TBufferWriter : IBufferWriter<byte>;

		bool TryDeserialize<TBufferWriter>(ref StreamDataStreamer<TBufferWriter> dataStreamer, ref TMessage instance)
			where TBufferWriter : IBufferWriter<byte>;
	}
}