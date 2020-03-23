using System;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct SpanScriber
	{
		public Span<byte> Span;
		public int Position;

		public SpanScriber(Span<byte> span)
		{
			Span = span;
			Position = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryWrite(byte value)
		{
			if (Position < Span.Length)
			{
				Span[Position] = value;
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryWrite(ReadOnlyMemory<byte> data)
			=> data.Span.TryCopyTo(Span.Slice(Position));
	}
}