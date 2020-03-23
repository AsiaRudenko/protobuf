using FluentAssertions;

using Xunit;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Tests.Serialization
{
	public class ExactSizeTests
	{
		// TODO: more test cases
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
		public void Varint32Size(uint result, byte[] data)
		{
			var size = ExactSize.VarintSize(result);

			size.Should().Be(data.Length);
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
		public void Varint64Size(ulong result, byte[] data)
		{
			var size = ExactSize.VarintSize(result);

			size.Should().Be(data.Length);
		}
	}
}