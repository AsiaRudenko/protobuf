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
		// there might be some faster method that reads it in reverse, i dunno
		public static DataDecodeResult<uint> TryReadVarint32(ReadOnlySpan<byte> bytes)
		{
			if (bytes.Length == 0) return default;

			byte b;
			uint result;

			// to prevent branching, instead of using some kind of loop, we can just unroll
			// the whole thing
			b = bytes[0];
			result = b & 0b01111111u;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<uint>(1, result); // if there's not more, we're done

			// TODO: these length checks could be costing us some performance, perhaps they
			// could be minified somehow?
			if (bytes.Length < 1) return default; // if there's suppose to be more, we failed

			b = bytes[1];
			result |= unchecked((b & 0b01111111u) << 7);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<uint>(2, result);
			if (bytes.Length < 2) return default;

			b = bytes[2];
			result |= unchecked((b & 0b01111111u) << (7 * 2));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<uint>(3, result);
			if (bytes.Length < 3) return default;

			b = bytes[3];
			result |= unchecked((b & 0b01111111u) << (7 * 3));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<uint>(4, result);
			if (bytes.Length < 4) return default;

			b = bytes[4];
			result |= unchecked((b & 0b01111111u) << (7 * 4));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<uint>(5, result);
			return default; // can't read any more
		}

		public static DataDecodeResult<ulong> TryReadVarint64(ReadOnlySpan<byte> bytes, ulong k)
		{
			if (bytes.Length == 0) return default;

			byte b;
			ulong result;

			// to prevent branching, instead of using some kind of loop, we can just unroll
			// the whole thing
			b = bytes[0];
			result = b & 0b01111111uL;
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(1, result); // if there's not more, we're done

			// TODO: these length checks could be costing us some performance, perhaps they
			// could be minified somehow?
			if (bytes.Length < 1) return default; // if there's suppose to be more, we failed

			b = bytes[1];
			result |= unchecked((b & 0b01111111uL) << 7);
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(2, result);
			if (bytes.Length < 2) return default;

			b = bytes[2];
			result |= unchecked((b & 0b01111111uL) << (7 * 2));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(3, result);
			if (bytes.Length < 3) return default;

			b = bytes[3];
			result |= unchecked((b & 0b01111111uL) << (7 * 3));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(4, result);
			if (bytes.Length < 4) return default;

			b = bytes[4];
			result |= unchecked((b & 0b01111111uL) << (7 * 4));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(5, result);
			if (bytes.Length < 5) return default;

			// note to the reader: i *did* try to reuse the above method and do bit shifting
			// and whatnot, and it turned out to be a massive pain, so... here we are

			b = bytes[5];
			result |= unchecked((b & 0b01111111uL) << (7 * 5));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(6, result);
			if (bytes.Length < 6) return default;

			b = bytes[6];
			result |= unchecked((b & 0b01111111uL) << (7 * 6));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(7, result);
			if (bytes.Length < 7) return default;

			b = bytes[7];
			result |= unchecked((b & 0b01111111uL) << (7 * 7));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(8, result);
			if (bytes.Length < 8) return default;

			b = bytes[8];
			result |= unchecked((b & 0b01111111uL) << (7 * 8));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(9, result);
			if (bytes.Length < 9) return default;

			b = bytes[9];
			result |= unchecked((b & 0b01111111uL) << (7 * 9));
			if ((b & 0b10000000) == 0) return new DataDecodeResult<ulong>(10, result);
			return default; // no more to read
		}
	}
}