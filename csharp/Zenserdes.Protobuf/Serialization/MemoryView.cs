using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	public struct MemoryView : IDataView
	{
		private readonly ReadOnlyMemory<byte> _memory;
		private int _position;

		public MemoryView(ReadOnlyMemory<byte> memory)
		{
			_memory = memory;
			_position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> ReadBytes(int bytes)
			=> ReadBytesToMemory(bytes).Span;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytes(Span<byte> target)
		{
			var slice = ReadBytes(target.Length);
			slice.CopyTo(target);
			return slice.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlyMemory<byte> ReadBytesToMemory(int bytes)
		{
			var slice = _memory.Slice(_position, bytes);
			return slice;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytesToMemory(Memory<byte> target)
		{
			var slice = ReadBytesToMemory(target.Length);
			slice.CopyTo(target);
			return slice.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			_position += bytes;
		}
	}
}