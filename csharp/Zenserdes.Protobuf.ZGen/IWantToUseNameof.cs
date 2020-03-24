using System;
using System.Buffers;
using Zenserdes.Protobuf.Serialization;

#nullable enable

namespace Zenserdes.Protobuf.ZGen
{
	internal struct IWantToUseNameof : IMessageAndOperator<IWantToUseNameof>
	{
		public int SizeHint => throw new NotImplementedException();

		public ulong ExactSize(in IWantToUseNameof message) => throw new NotImplementedException();
		public ulong ExactSize(IWantToUseNameof message) => throw new NotImplementedException();
		public bool Serialize(MemoryScriber target) => throw new NotImplementedException();

		public bool Serialize(SpanScriber target) => throw new NotImplementedException();

		public void Serialize(StreamScriber target) => throw new NotImplementedException();
		public bool TryDeserialize<TBufferWriter>(ref MemoryDataStreamer<TBufferWriter> dataStreamer, ref IWantToUseNameof instance) where TBufferWriter : IBufferWriter<byte> => throw new NotImplementedException();
		public bool TryDeserialize<TBufferWriter>(ref SpanDataStreamer<TBufferWriter> dataStreamer, ref IWantToUseNameof instance) where TBufferWriter : IBufferWriter<byte> => throw new NotImplementedException();
		public bool TryDeserialize<TBufferWriter>(ref StreamDataStreamer<TBufferWriter> dataStreamer, ref IWantToUseNameof instance) where TBufferWriter : IBufferWriter<byte> => throw new NotImplementedException();
	}
}