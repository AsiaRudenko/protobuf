using System;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public static class DataEncoder
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint EncodeZizZag32(int value)
			=> (uint)((value << 1) ^ (value >> 31));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong EncodeZizZag64(long value)
			=> (ulong)((value << 1) ^ (value >> 63));

		// varint implementation loosely based off of microsoft's implementation
		// https://source.dot.net/#System.Private.CoreLib/BinaryWriter.cs,456
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TrWriteVarint32(Span<byte> span, uint value, ref int offset)
		{
			// note to readers: the placement of these if statements may throw you off, but
			// they're ideal (i think, haven't benchmarked). the alternate placement is marked
			// with `//! here` and //! there`
			if (offset >= span.Length) return false;

			while (value >= 0b10000000)
			{
				//! here
				span[offset++] = (byte)(value | 0b10000000);
				value >>= 7;
				if (offset >= span.Length) return false;
			}

			//! there
			span[offset++] = (byte)value;
			return true;
		}

		public static bool TryWriteVarint64(Span<byte> span, ulong value, ref int offset)
		{
			if (offset >= span.Length) return false;

			while (value >= 0b10000000)
			{
				//! here
				span[offset++] = (byte)(value | 0b10000000);
				value >>= 7;
				if (offset >= span.Length) return false;
			}

			//! there
			span[offset++] = (byte)value;
			return true;
		}
	}
}