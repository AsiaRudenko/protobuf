using System;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct MemoryScriber
	{
		public Memory<byte> Memory;
		public Span<byte> Span;
		public int Position;

		public MemoryScriber(Memory<byte> memory)
		{
			Memory = memory;
			Span = Memory.Span;
			Position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryWrite(byte value)
		{
			if (Position < Memory.Length)
			{
				Span[Position] = value;
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryWrite(ReadOnlyMemory<byte> data)
			=> data.TryCopyTo(Memory.Slice(Position));
	}
}