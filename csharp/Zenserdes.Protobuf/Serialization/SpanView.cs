using System;
using System.Buffers;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	// TODO: figure out proper API usage
	//
	// TODO: change internal -> public when it works
	// vvvvv
	internal ref struct SpanView<TBufferWriter> // : IDataView
		where TBufferWriter : IBufferWriter<byte>
	{
		private readonly ReadOnlySpan<byte> _span;
		private readonly TBufferWriter _bufferWriter;
		private int _position;

		public SpanView(ReadOnlySpan<byte> span, TBufferWriter bufferWriter)
		{
			_span = span;
			_bufferWriter = bufferWriter;
			_position = 0;
		}

		// TODO: because this is a ref struct and it can't implement IDataView anyways,
		// it's better (less copying) for the methods to stay like this.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<byte> ReadBytes(int bytes)
		{
			var slice = _span.Slice(_position, bytes);
			return slice;
		}

		// However, this one, it may be looked into if the method signature should change.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlyMemory<byte> ReadBytesToMemory(int bytes)
		{
			var slice = ReadBytes(bytes);

			var memory = _bufferWriter.GetMemory(bytes);
			slice.CopyTo(memory.Span);
			_bufferWriter.Advance(bytes);

			return memory;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			_position += bytes;
		}
	}
}