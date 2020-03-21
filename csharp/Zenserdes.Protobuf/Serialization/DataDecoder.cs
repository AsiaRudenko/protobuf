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
		public static int DecodeZigZag32(int value)
			=> (value >> 1) ^ -(value & 1);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long DecodeZigZag64(long value)
			=> (value >> 1) & -(value & 1);

		// varint implementation loosely based off of microsoft's implementation
		// https://source.dot.net/#System.Private.CoreLib/BinaryReader.cs,587
		// there might be some faster method that reads it in reverse, i dunno
		public static DataDecodeResult<int> TryReadVarint32(ReadOnlySpan<byte> bytes)
		{
			if (bytes.Length == 0) return new DataDecodeResult<int>(0);

			byte b;
			int result;

			// to prevent branching, instead of using some kind of loop, we can just unroll
			// the whole thing
			b = bytes[0];
			result = b & 0b01111111;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<int>(1, result); // if there's not more, we're done

			// TODO: these length checks could be costing us some performance, perhaps they
			// could be minified somehow?
			if (bytes.Length < 1) return default; // if there's suppose to be more, we failed

			b = bytes[1];
			result |= (b & 0b01111111) << 7;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<int>(2, result);
			if (bytes.Length < 2) return default;

			b = bytes[2];
			result |= (b & 0b01111111) << (7 * 2);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<int>(3, result);
			if (bytes.Length < 3) return default;

			b = bytes[3];
			result |= (b & 0b01111111) << (7 * 3);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<int>(4, result);
			if (bytes.Length < 4) return default;

			b = bytes[4];
			result |= (b & 0b01111111) << (7 * 4);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<int>(5, result);
			return default; // can't read any more
		}

		public static DataDecodeResult<long> TryReadVarint64(ReadOnlySpan<byte> bytes)
		{
			if (bytes.Length == 0) return new DataDecodeResult<long>(0);

			byte b;
			int result;

			// TODO: perhaps code size could be shrank? for now it's a moot point

			/* COPIED & PASTED SECTION START */
			b = bytes[0];
			result = b & 0b01111111;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<long>(1, result); // if there's not more, we're done
			if (bytes.Length < 1) return default; // if there's suppose to be more, we failed

			b = bytes[1];
			result |= (b & 0b01111111) << 7;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<long>(2, result);
			if (bytes.Length < 2) return default;

			b = bytes[2];
			result |= (b & 0b01111111) << (7 * 2);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<long>(3, result);
			if (bytes.Length < 3) return default;

			b = bytes[3];
			result |= (b & 0b01111111) << (7 * 3);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<long>(4, result);
			if (bytes.Length < 4) return default;

			b = bytes[4];
			result |= (b & 0b01111111) << (7 * 4);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<long>(5, result);
			/* COPIED & PASTED SECTION END */

			// still more left - use the ReadVarint32 implementation
			var remaining = TryReadVarint32(bytes.Slice(5));
			if (remaining.BytesRead == 0) return default;

			long longTotal = (result << (7 * remaining.BytesRead)) | remaining.Value;
			return new DataDecodeResult<long>(5 + remaining.BytesRead, longTotal);
		}
	}
}