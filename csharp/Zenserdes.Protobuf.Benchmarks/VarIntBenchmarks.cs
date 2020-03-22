using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Benchmarks
{
	/// <summary>
	/// These benchmarks were to test how efficient differnet variable integer reading
	/// algorithms were.
	/// </summary>
	public class VarIntBenchmarks
	{
		[Params(new byte[] { 0b0111_1111 },
			new byte[] { 0b1010_1100, 0b0000_0010 },
			new byte[] { 0b1100_0010, 0b1010_0110, 0b0010_0101 },
			new byte[] { 0b1010_1110, 0b1010_0101, 0b1001_0101, 0b0101_1110 })]
		public byte[] _varint;

		[Benchmark]
		public int ReadVarIntATHP2ss()
		{
			uint result = 0;
			var success = ReadVarIntATHP(_varint, ref result);

			Debug.Assert(success);
			return (int)result;
		}

		[Benchmark]
		public int ReadVarIntM33()
		{
			uint result = 0;
			var success = ReadVarIntM3(_varint, ref result);

			Debug.Assert(success);
			return (int)result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadVarIntM3(ReadOnlySpan<byte> span, ref uint data)
		{
			if (span.IsEmpty) return false;
			var b = span[0];
			data |= (b & 0b01111111u);

			if ((b & 0b1000_0000) == 0) return true;

			var offset = 1;
			var shift = 7;

			do
			{
				if (offset >= span.Length) return false;
				if (shift == 5 * 7) return false;

				b = span[offset++];

				var resultData = (b & 0b01111111u);
				data |= resultData << shift;
				shift += 7;
			}
			while ((b & 0b1000_0000) != 0);

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ReadVarIntATHP(ReadOnlySpan<byte> span, ref uint data)
		{
			if (span.IsEmpty) return false;
			var b = span[0];
			data |= (b & 0b01111111u);

			if ((b & 0b1000_0000) == 0) return true;
			if (span.Length == 1) return false;
			b = span[1];
			data |= (b & 0b01111111u) << 7;

			if ((b & 0b1000_0000) == 0) return true;
			if (span.Length == 2) return false;
			b = span[2];
			data |= (b & 0b01111111u) << (7 * 2);

			if ((b & 0b1000_0000) == 0) return true;
			if (span.Length == 3) return false;
			b = span[3];
			data |= (b & 0b01111111u) << (7 * 3);

			if ((b & 0b1000_0000) == 0) return true;
			if (span.Length == 4) return false;
			b = span[4];
			data |= (b & 0b01111111u) << (7 * 4);

			if ((b & 0b1000_0000) == 0) return true;
			return false;
		}
	}
}
