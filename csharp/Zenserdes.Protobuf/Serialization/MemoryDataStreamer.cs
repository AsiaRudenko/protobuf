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

		public ReadOnlySpan<byte> MaybeReadWorkaround(int max)
			=> ReadOnlySpan.Slice(Position, Math.Min(ReadOnlySpan.Length - Position, max));

		/// <summary>
		/// Only used for varint decoding. This isn't very useful in any other situation,
		/// because there is no way to tell how many bytes were copied.
		/// </summary>
		/// <param name="target"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MaybeRead(Span<byte> target)
		{
			ReadOnlySpan.Slice(Position).CopyTo(target);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			Position += bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadPermanent(int bytes, out ReadOnlyMemory<byte> memory)
		{
			if (Position + bytes <= ReadOnlyMemory.Length)
			{
				memory = ReadOnlyMemory.Slice(Position, bytes);
				Position += bytes;
				return true;
			}

			memory = default;
			return false;
		}
	}
}