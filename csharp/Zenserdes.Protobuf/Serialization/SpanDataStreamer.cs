using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct SpanDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		public SpanDataStreamer(ReadOnlySpan<byte> span, TBufferWriter bufferWriter)
		{
		}
	}
}
