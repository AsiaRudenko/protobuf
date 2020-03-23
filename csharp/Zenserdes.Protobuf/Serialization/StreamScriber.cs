using System;
using System.IO;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct StreamScriber
	{
		private readonly Stream _stream;

		public StreamScriber(Stream stream)
		{
			_stream = stream;
		}

		// TODO: implement buffer
		// for now, we hope the user wraps their stream in a BufferedStream

		public bool TryWrite(byte @value)
		{
			_stream.WriteByte(@value);
			return true;
		}

		public bool TryWrite(ReadOnlyMemory<byte> value)
		{
			_stream.Write(value.Span);
			return true;
		}
	}
}