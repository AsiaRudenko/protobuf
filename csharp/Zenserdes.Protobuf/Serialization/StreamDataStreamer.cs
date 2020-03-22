using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct StreamDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		public StreamDataStreamer(Stream stream, TBufferWriter bufferWriter)
		{
		}
	}
}
