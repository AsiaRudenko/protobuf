using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Zenserdes.Protobuf.Serialization
{
	// TODO: once I get the DataStreamer laid out, i can more efficiently optimize
	// all the data views.

	/// <summary>
	/// Can stream the data of a protobuf message until the end.
	/// </summary>
	/// <typeparam name="TDataView">The type of data view to use to read the data.</typeparam>
	public ref struct DataStreamer<TDataView>
		where TDataView : IDataView
	{
		private TDataView _dataView; // * DO NOT make readonly

		public DataStreamer(TDataView dataView)
		{
			_dataView = dataView;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			_dataView.Advance(bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte NextSegment(ref ReadOnlyMemory<byte> data)
		{
			// TODO: have special "ReadByte()" method to optimize this case
			var wireBytes = _dataView.ReadBytes(1);
			if (wireBytes.Length == 0) return default;

			var wireByte = wireBytes[0];

			// TODO: do we really need to do this? it's a waste of instructions if the user
			// doesn't use it.
			var fieldNumber = 0; // (byte)((wireByte & 0b11111_000) >> 3);
			var wireType = (byte)(wireByte & 0b00000_111);

			_dataView.Advance(1);

			_lookupActions[wireType](ref this, ref data);
			return wireByte;
		}

		// Lookup table stuff
		//
		// For memory efficiency, it'd probably be better to put this in a non generic
		// class, but since there are only 2^3 (8) possible options, we're fine.
		private delegate void LookupAction(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment);

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
		private static void Fail(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment)
		{
			// we shouldn't need to assign Success, it should already be false.
			// so we'll avoid setting the field.

			// in release, it'd be nice to optimize this function out of the equation as
			// much as possible.
#if DEBUG
			Debug.Assert(segment.IsEmpty);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Varint(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment)
		{
			// really, we wish to leave this up to the caller on how it should be decoded.
			// we cannot accurately make the assumption on whether to use the varint32 or
			// varint64 decoder, and incorrect usage would be a performance hit.
			//
			// we shall just try to read 10 bytes (varint64 max size), and yield that.

			segment = dataStreamer._dataView.ReadBytesToMemory(10);

			// TODO: figure out exactly how many bytes we need to advance without decoding
			// dataStreamer._dataView.Advance(r.BytesRead);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Bit64(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment)
		{
			segment = dataStreamer._dataView.ReadBytesToMemory(8);

			if (segment.Length < 8)
			{
				segment = default;
				return;
			}

			dataStreamer._dataView.Advance(8);
		}

		// TODO: move somewhere else?
		/// <summary>
		/// Because a protobuf message can have length delimited sections that are very
		/// large, if the library was to allocate the chunks of memory requested very
		/// frequently, it would result in an exception (which this library aims to not
		/// throw!).
		/// </summary>
		public static int MaximumLengthDelimitedReadSize = 32_000_000;

		private static void LengthDelimited(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment)
		{
			var lengthBytes = dataStreamer._dataView.ReadBytes(5);
			var bytesRead = DataDecoder.TryReadVarint32(lengthBytes, out var uLength);

			if (bytesRead == 0)
			{
				//# segment.Success = false;
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

			if (uLength >= MaximumLengthDelimitedReadSize)
			{
				//# segment.Success = false;
				return;
			}

			var length = (int)uLength;

			dataStreamer._dataView.Advance(bytesRead);
			segment = dataStreamer._dataView.ReadBytesToMemory(length); // TODO: should we read for permanence?

			if (segment.Length < length)
			{
				segment = default;
				return;
			}

			dataStreamer._dataView.Advance(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Bit32(ref DataStreamer<TDataView> dataStreamer, ref ReadOnlyMemory<byte> segment)
		{
			segment = dataStreamer._dataView.ReadBytesToMemory(4);

			if (segment.Length < 4)
			{
				segment = default;
				return;
			}

			dataStreamer._dataView.Advance(4);
		}
	}
}