using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct StreamDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		public Stream Stream;
		public TBufferWriter BufferWriter;

		public StreamDataStreamer(Stream stream, TBufferWriter bufferWriter)
		{
			Stream = stream;
			BufferWriter = bufferWriter;
		}

		// while it'd be very performant to buffer the stream for the user, i'd rather
		// let the user handle that.
		//
		// TODO: make reading from streams more efficient.

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool Next(ref byte data)
		{
			var target = new Span<byte>(Unsafe.AsPointer(ref data), 1);

			return Stream.Read(target) == 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadPermanent(int bytes, out Memory<byte> memory)
		{
			memory = BufferWriter.GetMemory(bytes).Slice(0, bytes);

			var bytesRead = Stream.Read(memory.Span);

			if (bytesRead != bytes)
			{
				return false;
			}

			BufferWriter.Advance(bytes);
			return true;
		}
	}
}