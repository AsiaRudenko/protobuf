using BenchmarkDotNet.Attributes;

using System;
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
			var streamer = new DataStreamer<MemoryView>(new MemoryView(_payload));

			ReadOnlyMemory<byte> segment = default;
			byte wireByte = default;

			wireByte = streamer.NextSegment(ref segment);
			if (segment.IsEmpty) return request;
			request.HandleDeserialization(ref segment, wireByte);

			wireByte = streamer.NextSegment(ref segment);
			if (segment.IsEmpty) return request;
			var adv = request.HandleDeserialization(ref segment, wireByte);
			streamer.Advance(adv);

			wireByte = streamer.NextSegment(ref segment);
			if (segment.IsEmpty) return request;
			request.HandleDeserialization(ref segment, wireByte);

			return request;
		}
	}

	public struct SearchRequest // : IZenserdesProtobufMessage
	{
		public int SizeHint => throw new NotImplementedException();

		public ReadOnlyMemory<byte> Query { get; set; }
		public int PageNumber { get; set; }
		public int ResultsPerPage { get; set; }

		// TODO: return boolean or something
		public int HandleDeserialization(ref ReadOnlyMemory<byte> segment, byte wireByte)
		{
			if (wireByte >= _lookupTable.Length)
			{
				return 0;
			}
			/*
			switch (segment.WireByte)
			{
				case 10: return SetQuery(ref this, ref segment.Data);
				case 16: return SetPageNumber(ref this, ref segment.Data);
				case 24: return SetResultsPerPage(ref this, ref segment.Data);
				default: return 0;
			}
			*/
			return _lookupTable[wireByte](ref this, ref segment);
		}

		private delegate int LookupTableAction(ref SearchRequest searchRequest, ref ReadOnlyMemory<byte> data);

		private static LookupTableAction[] _lookupTable = new LookupTableAction[]
		{
			Fail, Fail, Fail, Fail, Fail, Fail, Fail, Fail, Fail, Fail,

			// index 10: field 1, wire type 2, 'Query'
			SetQuery,

			Fail, Fail, Fail, Fail, Fail,

			// index 16: field 2, wire type 0, 'PageNumber'
			SetPageNumber,

			Fail, Fail, Fail, Fail, Fail, Fail, Fail,

			// index 24: field 3, wire type 0, 'ResultsPerPage'
			SetResultsPerPage
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int Fail(ref SearchRequest searchRequest, ref ReadOnlyMemory<byte> data)
		{
			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int SetQuery(ref SearchRequest searchRequest, ref ReadOnlyMemory<byte> data)
		{
			searchRequest.Query = data;
			return default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int SetPageNumber(ref SearchRequest searchRequest, ref ReadOnlyMemory<byte> data)
		{
			var bytesRead = DataDecoder.TryReadVarint32(data.Span, out var result);

			if (bytesRead == 0)
			{
				// TODO: return false
				return default;
			}

			searchRequest.PageNumber = unchecked((int)result);
			return bytesRead;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int SetResultsPerPage(ref SearchRequest searchRequest, ref ReadOnlyMemory<byte> data)
		{
			var bytesRead = DataDecoder.TryReadVarint32(data.Span, out var result);

			if (bytesRead == 0)
			{
				// TODO: return false
				return default;
			}

			searchRequest.ResultsPerPage = unchecked((int)result);
			return bytesRead;
		}
	}
}