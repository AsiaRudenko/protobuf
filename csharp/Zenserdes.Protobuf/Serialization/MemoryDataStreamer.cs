using System;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct MemoryDataStreamer
	{
		public readonly ReadOnlyMemory<byte> ReadOnlyMemory;
		public readonly ReadOnlySpan<byte> ReadOnlySpan;
		public int Position;

		public MemoryDataStreamer(ReadOnlyMemory<byte> rom)
		{
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