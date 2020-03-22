using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct DataDecodeResult<T>
	{
		/// <summary>
		/// 0 if the decode failed.
		/// </summary>
		public int BytesRead;

		public T Value;

		public DataDecodeResult(int bytesRead, T value = default)
		{
			BytesRead = bytesRead;
			Value = value;
		}
	}

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
		public static int TryReadVarint32(ReadOnlySpan<byte> bytes, out uint result)
		{
			var offset = 0;
			byte b;
			result = 0;

			do
			{
				if (offset == bytes.Length) return default;
				if (offset == 5) return default;

				b = bytes[offset];
				result |= (b & 0b01111111u) << (offset * 7); // TODO: is offset * 7 faster than shift += 7?
				offset++;
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
		public static int TryReadVarint322(ReadOnlySpan<byte> bytes, out uint result)
		{
			var offset = 0;
			var shift = 0;
			byte b;
			result = 0;

			do
			{
				if (offset == bytes.Length) return default;
				if (offset == 5) return default;

				b = bytes[offset++];
				result |= (b & 0b01111111u) << shift; // TODO: is offset * 7 faster than shift += 7?
				shift += 7;
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
		private static int[] _lkp = new int[] { 7 * 0, 7 * 1, 7 * 2, 7 * 3, 7 * 4, 7 * 5, 7 * 6, 7 * 7, 7 * 8, 7 * 9, 7 * 10 };
		public static int TryReadVarint323(ReadOnlySpan<byte> bytes, out uint result)
		{
			var offset = 0;
			byte b;
			result = 0;

			do
			{
				if (offset == bytes.Length) return default;
				if (offset == 5) return default;

				b = bytes[offset];
				result |= (b & 0b01111111u) << _lkp[offset]; // TODO: is offset * 7 faster than shift += 7?
				offset++;
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
		public static int TryReadVarint324(ReadOnlySpan<byte> bytes, out uint result)
		{
			var offset = 0;
			byte b;
			result = 0;
			var cmp = Math.Min(5, bytes.Length);

			do
			{
				if (offset == cmp) return default;

				b = bytes[offset];
				result |= (b & 0b01111111u) << _lkp[offset]; // TODO: is offset * 7 faster than shift += 7?
				offset++;
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
		public static int TryReadVarint325(ReadOnlySpan<byte> bytes, out uint result)
		{
			var offset = 0;
			byte b;
			result = 0;
			var cmp = Math.Min(5, bytes.Length);

			do
			{
				if (offset == cmp) return default;

				b = bytes[offset];
				result |= (b & 0b01111111u) << (offset * 7); // TODO: is offset * 7 faster than shift += 7?
				offset++;
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
		public static int TryReadVarint326(ReadOnlySpan<byte> bytes, out uint result)
		{
			var shift = 0;
			var offset = 0;
			byte b;
			result = 0;

			do
			{
				if (offset == bytes.Length) return default;
				if (shift == 5 * 7) return default;

				b = bytes[offset];
				result |= (b & 0b01111111u) << shift; // TODO: is offset * 7 faster than shift += 7?
				offset++;
				shift += 7;
			}
			while ((b & 0b10000000) != 0);

			return shift / 7;
		}
		/*
		 * 

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadVarInt2(ReadOnlySpan<byte> span, ref ulong data, bool maxInt = false)
		{
			var max = 10 * 7; // maximum 10 bytes for a ulong
			if (maxInt) max = 5 * 7; // maximum 5 bytes for an int

			int shift = 0;
			byte b;

			var offset = 0;

			do
			{
				if (offset >= span.Length) return false;
				if (shift == max) return false;

				b = span[offset++];

				var resultData = (ulong)(b & 0b01111111);
				data |= resultData << shift;
				shift += 7;
			}
			while ((b & 0b1000_0000) != 0);
			if (maxInt && data > int.MaxValue) return false;

			return true;
		}
		*/

		public static int TryReadVarint64(ReadOnlySpan<byte> bytes, out ulong result)
		{
			var offset = 0;
			byte b;
			result = 0;

			do
			{
				if (offset < bytes.Length) return default;
				if (offset == 10) return default;

				b = bytes[offset++];
				result |= (b & 0b01111111u) << (offset * 7);
			}
			while ((b & 0b10000000) != 0);

			return offset;
		}
	}
}
 