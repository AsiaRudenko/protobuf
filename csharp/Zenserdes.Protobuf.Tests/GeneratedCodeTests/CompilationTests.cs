using FluentAssertions;

using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

using Zenserdes.Protobuf.Serialization;

namespace Zenserdes.Protobuf.Tests.GeneratedCodeTests
{
	public class CompilationTests : CompilationTest
	{
		public CompilationTests(ITestOutputHelper logger) : base(logger)
		{
		}

		[Fact]
		public Task BlankProtobuf_DoesntFail()
		{
			return RunCode("syntax = \"proto3\";", "");
		}

		[Fact]
		public Task BlankProtobuf_WithNamespace_DoesntFail()
		{
			return RunCode("syntax = \"proto3\";", "", "A.Cool.Namespace");
		}

		[Fact]
		public Task EmptyMessage_UsingShould_DoesntFail()
		{
			return RunCode(@"
syntax = ""proto3"";

message Blank
{
}
", @"
default(Blank)
	.Should().Be(default(Blank));
");
		}

		[Fact]
		public Task EmptyMessage_UsingShould_DoesntFail_WithNamespace()
		{
			return RunCode(@"
syntax = ""proto3"";

message Blank
{
}
", @"
default(Blank)
	.Should().Be(default(Blank));
", "With.A.Namespace");
		}

		[Fact]
		public Task EmptyMessage_WithInt_CanDeserialize()
		{
			var offset = 0;
			var result = 0u;
			var success = DataDecoder.TryReadVarint32(new byte[] { 0b00001_000, 0xFF, 0xFF, 0x00 }, ref offset, ref result);
			success.Should().BeTrue();

			return RunCode(@"
syntax = ""proto3"";

message MessageWithInt
{
	int32 value = 1;
}
", $@"
var payload = new byte[] {{ 0b00001_000, 0xFF, 0xFF, 0x00 }};
var success = ZenserdesProtobuf.TryDeserialize<MessageWithInt>(new ReadOnlyMemory<byte>(payload), out var instance);

success.Should().BeTrue();
instance.Value.Should().Be({(int)result});
");
		}
	}
}