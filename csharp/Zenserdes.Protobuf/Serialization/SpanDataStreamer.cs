using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct SpanDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		public readonly ReadOnlySpan<byte> Span;

		// *not* marked as readonly, incase it is a mutating struct
		public TBufferWriter BufferWriter;

		public int Position;

		public SpanDataStreamer(ReadOnlySpan<byte> span, TBufferWriter bufferWriter)
		{
			Span = span;
			BufferWriter = bufferWriter;
			Position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Next(ref byte data)
		{
			if (Position < Span.Length)
			{
				data = Span[Position++];
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadPermanent(int bytes, out Memory<byte> memory)
		{
			if (Position + bytes <= Span.Length)
			{
				memory = BufferWriter.GetMemory(bytes).Slice(0, bytes);
				BufferWriter.Advance(bytes);

				Span.Slice(Position, bytes).CopyTo(memory.Span);
				Position += bytes;
				return true;
			}

			memory = default;
			return false;
		}
	}
}