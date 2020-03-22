using FluentAssertions;

using Xunit;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Tests.Serialization
{
	public class DataDecoderTests
	{
		// test data: https://github.com/protocolbuffers/protobuf/blob/70fc0f0275389a99b0c654778ef937f904921be0/csharp/src/Google.Protobuf.Test/CodedOutputStreamTest.cs#L214-L221
		[Theory]
		[InlineData(0u, 0)]
		[InlineData(1u, -1)]
		[InlineData(2u, 1)]
		[InlineData(3u, -2)]
		[InlineData(0x7FFFFFFEu, 0x3FFFFFFF)]
		[InlineData(0x7FFFFFFFu, unchecked((int)0xC0000000))]
		[InlineData(0xFFFFFFFEu, 0x7FFFFFFF)]
		[InlineData(0xFFFFFFFFu, unchecked((int)0x80000000))]
		public void ZigZag32Decode_IsCorrect(uint value, int decoded)
		{
			DataDecoder.DecodeZigZag32(value).Should().Be(decoded);
		}

		[Theory]
		// https://developers.google.com/protocol-buffers/docs/encoding#varints
		[InlineData(1, new byte[] { 0b0000_0001 })]
		[InlineData(300, new byte[] { 0b1010_1100, 0b0000_0010 })]
		// test data: https://github.com/protocolbuffers/protobuf/blob/70fc0f0275389a99b0c654778ef937f904921be0/csharp/src/Google.Protobuf.Test/CodedOutputStreamTest.cs#L101-L109
		[InlineData(0, new byte[] { 0b00000000 })]
		[InlineData(127, new byte[] { 0x7F })]
		[InlineData((0x22 << 0) | (0x74 << 7), new byte[] { 0xA2, 0x74 })]
		[InlineData((0x3E << 0) | (0x77 << 7) | (0x12 << 14) | (0x04 << 21) | (0x0Bu << 28),
			new byte[] { 0xBE, 0xF7, 0x92, 0x84, 0x0B })]
		public void Varint32Decode_IsCorrect(uint result, byte[] data)
		{
			var decode = DataDecoder.TryReadVarint32(data);

			decode.BytesRead.Should().Be(data.Length);
			decode.Value.Should().Be(result);
		}

		// test data: https://github.com/protocolbuffers/protobuf/blob/70fc0f0275389a99b0c654778ef937f904921be0/csharp/src/Google.Protobuf.Test/CodedOutputStreamTest.cs#L111-L127
		[Theory]
		[InlineData(7256456126uL,
			new byte[] { 0xBE, 0xF7, 0x92, 0x84, 0x1B })]
		[InlineData(41256202580718336uL,
			new byte[] { 0x80, 0xE6, 0xEB, 0x9C, 0xC3, 0xC9, 0xA4, 0x49 })]
		[InlineData(11964378330978735131uL,
			new byte[] { 0x9B, 0xA8, 0xF9, 0xC2, 0xBB, 0xD6, 0x80, 0x85, 0xA6, 0x01 })]
		public void Varint64Decode_IsCorrect(ulong result, byte[] data)
		{
			var decode = DataDecoder.TryReadVarint64(data, result);

			decode.BytesRead.Should().Be(data.Length);
			decode.Value.Should().Be(result);
		}
	}
}