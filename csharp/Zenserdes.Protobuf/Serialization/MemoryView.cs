using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	public struct MemoryView : IDataView
	{
		private ReadOnlyMemory<byte> _memory;

		public MemoryView(ReadOnlyMemory<byte> memory)
		{
			_memory = memory;
			// _position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> ReadBytes(int bytes)
			=> ReadBytesToMemory(bytes).Span;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytes(Span<byte> target)
		{
			throw new Exception();
			var slice = ReadBytes(target.Length);
			slice.CopyTo(target);
			return slice.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlyMemory<byte> ReadBytesToMemory(int bytes)
		{
			return _memory; // .Slice(_position);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int ReadBytesToMemory(Memory<byte> target)
		{
			throw new Exception();
			var slice = ReadBytesToMemory(target.Length);
			slice.CopyTo(target);
			return slice.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			_memory = _memory.Slice(bytes);
			// _position += bytes;
		}
	}
}