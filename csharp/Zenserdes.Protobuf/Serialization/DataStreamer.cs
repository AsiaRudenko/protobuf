using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zenserdes.Protobuf.Serialization
{
	[StructLayout(LayoutKind.Explicit)]
	public ref struct ProtobufMessageSegment
	{
		public ProtobufMessageSegment(byte fieldNumber, byte wireType)
		{
			FieldNumber = fieldNumber;
			WireType = wireType;

			Success = default;
			Data = default;
		}

		// TODO: instead of a boolean, have it be an enum so the user can debug why a
		// given protobuf message didn't deserialize.
		[FieldOffset(0)] public bool Success;

		[FieldOffset(1)] public byte FieldNumber;
		[FieldOffset(2)] public byte WireType;

		// extra (1) byte wasted for ideal struct alignment

		[FieldOffset(4)] public ReadOnlySpan<byte> Data;
	}

	// TODO: once I get the DataStreamer laid out, i can more efficiently optimize
	// all the data views.

	/// <summary>
	/// Can stream the data of a protobuf message until the end.
	/// </summary>
	/// <typeparam name="TDataView">The type of data view to use to read the data.</typeparam>
	public struct DataStreamer<TDataView>
		where TDataView : IDataView
	{
		private readonly TDataView _dataView;

		public DataStreamer(TDataView dataView)
		{
			_dataView = dataView;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ProtobufMessageSegment NextSegment()
		{
			// TODO: have special "ReadByte()" method to optimize this case
			var wireByte = _dataView.ReadBytes(1)[0];

			// TODO: do we really need to do this? it's a waste of instructions if the user
			// doesn't use it.
			var fieldNumber = (byte)((wireByte & 0b11111_000) >> 3);
			var wireType = (byte)(wireByte & 0b00000_111);

			_dataView.Advance(1);

			var segment = new ProtobufMessageSegment(fieldNumber, wireType);
			_lookupActions[wireType](ref this, ref segment);
			return segment;
		}

		// Lookup table stuff
		//
		// For memory efficiency, it'd probably be better to put this in a non generic
		// class, but since there are only 2^3 (8) possible options, we're fine.
		private delegate void LookupAction(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment);

		private static readonly LookupAction[] _lookupActions = new LookupAction[]
		{
			Varint,
			Bit64,
			LengthDelimited,
			Fail, // start group: deprecated
			Fail, // end group: deprecated
			Bit32,

			Fail, // padding
			Fail, // padding
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Fail(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment)
		{
			// we shouldn't need to assign Success, it should already be false.
			// so we'll avoid setting the field.

			// in release, it'd be nice to optimize this function out of the equation as
			// much as possible.
#if DEBUG
			Debug.Assert(segment.Success == false);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Varint(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment)
		{
			// really, we wish to leave this up to the caller on how it should be decoded.
			// we cannot accurately make the assumption on whether to use the varint32 or
			// varint64 decoder, and incorrect usage would be a performance hit.
			//
			// we shall just try to read 10 bytes (varint64 max size), and yield that.

			var data = dataStreamer._dataView.ReadBytes(10);

			if (data.Length == 0)
			{
				segment.Success = false;
				return;
			}

			dataStreamer._dataView.Advance(10);
			segment.Data = data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Bit64(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment)
		{
			var data = dataStreamer._dataView.ReadBytes(8);

			if (data.Length != 8)
			{
				segment.Success = false;
				return;
			}

			dataStreamer._dataView.Advance(8);
			segment.Data = data;
		}

		// TODO: move somewhere else?
		/// <summary>
		/// Because a protobuf message can have length delimited sections that are very
		/// large, if the library was to allocate the chunks of memory requested very
		/// frequently, it would result in an exception (which this library aims to not
		/// throw!).
		/// </summary>
		public static int MaximumLengthDelimitedReadSize = 32_000_000;

		private static void LengthDelimited(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment)
		{
			var lengthBytes = dataStreamer._dataView.ReadBytes(5);
			var varint = DataDecoder.TryReadVarint32(lengthBytes);

			if (varint.BytesRead == 0)
			{
				segment.Success = false;
				return;
			}

			// if the varint is too big, we should reject the protobuf message. Otherwise
			// we might crash the user of the library because it allocates too much memory.
			//
			// TODO: have the data streamer keep track of how much memory it has allocated
			// and use that as the cutoff point for reading a message, maybe?
			//
			// the data streamer streams data, so it should be up to the caller if the memory
			// gets allocated or not.

			var uLength = varint.Value;
			if (uLength >= MaximumLengthDelimitedReadSize)
			{
				segment.Success = false;
				return;
			}

			var length = (int)uLength;

			dataStreamer._dataView.Advance(varint.BytesRead);
			var bytes = dataStreamer._dataView.ReadBytes(length); // TODO: should we read for permanence?

			if (bytes.Length != length)
			{
				segment.Success = false;
				return;
			}

			dataStreamer._dataView.Advance(length);
			segment.Success = true;
			segment.Data = bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Bit32(ref DataStreamer<TDataView> dataStreamer, ref ProtobufMessageSegment segment)
		{
			var data = dataStreamer._dataView.ReadBytes(4);

			if (data.Length != 4)
			{
				segment.Success = false;
				return;
			}

			dataStreamer._dataView.Advance(4);
			segment.Data = data;
		}
	}
}