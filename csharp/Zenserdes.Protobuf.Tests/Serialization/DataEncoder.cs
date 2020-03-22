using FluentAssertions;

using Xunit;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Tests.Serialization
{
	public class DataEncoderTests
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
		public void ZigZag32Encode_IsCorrect(uint encoded, int value)
		{
			DataEncoder.EncodeZizZag32(value).Should().Be(encoded);
		}
	}
}