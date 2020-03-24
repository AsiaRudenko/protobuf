using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct MemoryDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		// NOTE: BufferWriter isn't actually used for anything here. It's used in the
		// generated code when it needs to allocate a buffer for repeated numbers
		public TBufferWriter BufferWriter;

		public readonly ReadOnlyMemory<byte> ReadOnlyMemory;
		public readonly ReadOnlySpan<byte> ReadOnlySpan;
		public int Position;

		public MemoryDataStreamer(ReadOnlyMemory<byte> rom, TBufferWriter bufferWriter)
		{
			BufferWriter = bufferWriter;
			ReadOnlyMemory = rom;
			ReadOnlySpan = ReadOnlyMemory.Span;
			Position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Next(ref byte data)
		{
			if (Position < ReadOnlySpan.Length)
			{
				data = ReadOnlySpan[Position++];
				return true;
			}

			return false;
		}
	}
}