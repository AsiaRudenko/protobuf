using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	public static class DataDecoder
	{
		// based on Google DecodeZigZag32 & DecodeZizZag64
		// https://github.com/protocolbuffers/protobuf/blob/master/csharp/src/Google.Protobuf/CodedInputStream.cs#L1297-L1323
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DecodeZigZag32(uint value)
			=> (int)(value >> 1) ^ -((int)value & 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DecodeZigZag64(ulong value)
			=> (long)(value >> 1) ^ -(long)(value & 1);

		// varint implementation loosely based off of microsoft's implementation
		// https://source.dot.net/#System.Private.CoreLib/BinaryReader.cs,587
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryReadVarint32(ReadOnlySpan<byte> span, ref int offset, ref uint result)
		{
			if (span.IsEmpty) return false;
			var b = span[offset++];
			result |= (b & 0b01111111u);

			if ((b & 0b1000_0000) == 0) return true;

			var shift = 7;

			do
			{
				if (offset >= span.Length) return false;
				if (shift == 5 * 7) return false;

				b = span[offset++];

				var resultData = (b & 0b01111111u);
				result |= resultData << shift;
				shift += 7;
			}
			while ((b & 0b1000_0000) != 0);

			return true;
		}

		public static bool TryReadVarint64(ReadOnlySpan<byte> span, ref int offset, ref ulong result)
		{
			if (span.IsEmpty) return false;
			var b = span[offset++];
			result |= (b & 0b01111111uL);

			if ((b & 0b1000_0000) == 0) return true;

			var shift = 7;

			do
			{
				if (offset >= span.Length) return false;
				if (shift == 10 * 7) return false;

				b = span[offset++];

				var resultData = (b & 0b01111111uL);
				result |= resultData << shift;
				shift += 7;
			}
			while ((b & 0b1000_0000) != 0);

			return true;
		}
	}
}