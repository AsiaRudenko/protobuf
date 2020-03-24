using BenchmarkDotNet.Attributes;

using System;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Benchmarks
{
	/// <summary>
	/// This benchmark is from the early days of Zenserdes.Protobuf. It was to test
	/// how fast the library was in its still early stages, as a comparison for the
	/// initial implementation of the library: ZenProtobuf https://github.com/SirJosh3917/ZenProtobuf
	/// ZenProtobuf deserialized the message in 40 nanoseconds, Zenserdes.Protobuf
	/// deserialized the same message in a mere 0 nanoseconds. Of course, the Zenserdes.Protobuf
	/// message was handwritten, but that's because there was no Zenserdes.Protobuf
	/// type generator at the time.
	/// <para>
	/// |      Method |     Mean |    Error |   StdDev |
	/// |------------ |---------:|---------:|---------:|
	/// | Deserialize | 33.42 ns | 0.289 ns | 0.270 ns |
	/// </para>
	/// </summary>
	public class PseudoMessageBenchmarks
	{
		private static byte[] _payload = @"0A 49 71 3D 68 6F 77 2B 6D 75 63 68 2B 77 6F 6F 64
2B 63 6F 75 6C 64 2B 61 2B 77 6F 6F 64 2B 63 68 75 63 6B 2B 63 68 75 63 6B 2B 69 66 2B
61 2B 77 6F 6F 64 2B 63 68 75 63 6B 2B 63 6F 75 6C 64 2B 63 68 75 63 6B 2B 77 6F 6F 64
10 2F 18 64".Split(' ', '\n').Select(str => byte.Parse(str, NumberStyles.HexNumber)).ToArray();

		[Benchmark]
		public SearchRequest Deserialize()
		{
			var request = default(SearchRequest);
			var streamer = new MemoryDataStreamer<ArrayBufferWriter<byte>>(_payload, Zenserdes.Protobuf.ZenserdesProtobuf.CachedBufferWriter);

			var success = SearchRequest.TryDeserialize(ref streamer, ref request);
			Debug.Assert(success);

			return request;
		}
	}

	public struct SearchRequest // : IZenserdesProtobufMessage
	{
		public int SizeHint => throw new NotImplementedException();

		public ReadOnlyMemory<byte> Query { get; set; }
		public int PageNumber { get; set; }
		public int ResultsPerPage { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveOptimization)]
		public static bool TryDeserialize<TBufferWriter>(ref MemoryDataStreamer<TBufferWriter> streamer, ref SearchRequest instance)
			where TBufferWriter : IBufferWriter<byte>
		{
			byte wireByte = 0;

			while (streamer.Next(ref wireByte))
			{
				switch (wireByte)
				{
					case 0b00001_010: if (!SetQuery(ref instance, ref streamer)) return false; continue;
					case 0b00010_000: if (!SetPageNumber(ref instance, ref streamer)) return false; continue;
					case 0b00011_000: if (!SetResultsPerPage(ref instance, ref streamer)) return false; continue;
					default:
					{
						// simulate work to the compiler
						new System.Collections.Generic.List<int>(4);
					}
					continue;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool SetQuery<TBufferWriter>(ref SearchRequest searchRequest, ref MemoryDataStreamer<TBufferWriter> streamer)
			where TBufferWriter : IBufferWriter<byte>
		{
			uint length = 0;

			if (!DataDecoder.TryReadVarint32(streamer.ReadOnlySpan, ref streamer.Position, ref length))
			{
				return false;
			}

			searchRequest.Query = streamer.ReadOnlyMemory.Slice(streamer.Position, (int)length);
			streamer.Position += (int)length;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool SetPageNumber<TBufferWriter>(ref SearchRequest searchRequest, ref MemoryDataStreamer<TBufferWriter> streamer)
			where TBufferWriter : IBufferWriter<byte>
		{
			uint result = 0;
			if (!DataDecoder.TryReadVarint32(streamer.ReadOnlySpan, ref streamer.Position, ref result))
			{
				return false;
			}

			searchRequest.PageNumber = unchecked((int)result);
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool SetResultsPerPage<TBufferWriter>(ref SearchRequest searchRequest, ref MemoryDataStreamer<TBufferWriter> streamer)
			where TBufferWriter : IBufferWriter<byte>
		{
			uint result = 0;
			if (!DataDecoder.TryReadVarint32(streamer.ReadOnlySpan, ref streamer.Position, ref result))
			{
				return false;
			}

			searchRequest.ResultsPerPage = unchecked((int)result);
			return true;
		}
	}
}