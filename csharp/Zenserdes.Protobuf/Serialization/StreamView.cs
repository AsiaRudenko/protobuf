using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	// TODO: patch this class up
	// currently it does not work, but the general idea is here so i'll leave it
	//
	// TODO: change internal -> public when it works
	// vvvvv
	internal struct StreamView<TBufferWriter> : IDataView
		where TBufferWriter : IBufferWriter<byte>
	{
		private Memory<byte> _buffer;
		private int _position;

		public const int BufferSize = 4096;
		public const int StreamReadCutoffPoint = 1024;

		public StreamView(Stream stream, TBufferWriter bufferWriter)
		{
			Stream = stream;
			BufferWriter = bufferWriter;
			_position = 0;

			_buffer = default; // little hack to use RentFromBuffer
			_buffer = RentFromBuffer(BufferSize);
		}

		public Stream Stream { get; }
		public TBufferWriter BufferWriter { get; }

		public ReadOnlySpan<byte> ReadBytes(int bytes) => throw new NotImplementedException();

		public ReadOnlyMemory<byte> ReadBytesToMemory(int bytes) => throw new NotImplementedException();

		public int ReadBytes(Span<byte> target)
		{
			MaybeFillBuffer(target.Length);
			var filled = FillTargetFromBuffer(target);
			var remaining = target.Length - filled;

			if (remaining == 0)
			{
				return filled;
			}

			// we have more bytes we can read
			if (remaining <= StreamReadCutoffPoint)
			{
				// we're not at the cutoff point, so we'll read the stream into the buffer and
				// copy the initial data we need out of it.
				FillBuffer();
				var bytesFilled = FillTargetFromBuffer(target.Slice(remaining));
				return filled + bytesFilled;
			}

			// if we're at the cutoff point, we're just going to read the stream straight
			// into the target
			var read = Stream.Read(target.Slice(filled));
			return filled + read;
		}

		public int ReadBytesToMemory(Memory<byte> target)
			=> ReadBytes(target.Span);

		// Rents `size` bytes from the buffer, and advances it.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private Memory<byte> RentFromBuffer(int size)
		{
			var slice = BufferWriter.GetMemory(size).Slice(0, size);
			BufferWriter.Advance(size);
			return slice;
		}

		// Fills the `target` based on the data left in _buffer.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int FillTargetFromBuffer(Span<byte> target)
		{
			var remainingBuffer = _buffer.Slice(_position).Span;
			remainingBuffer.CopyTo(target);

			_position += remainingBuffer.Length;
			return remainingBuffer.Length;
		}

		// This *might* fill the buffer, based on the amount of bytes being requested.
		// Its behaviour is up to it to decide.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool MaybeFillBuffer(int bytes)
		{
			// we don't want to fill up the buffer if we have enough space left to read in
			// the amount of bytes
			if (_buffer.Length - _position >= bytes)
			{
				return false;
			}

			// if bytes is bigger than the buffer, we definitely don't want to fill the buffer
			if (bytes >= _buffer.Length)
			{
				return false;
			}

			// if bytes is not less than the cutoff, we'll let the method read from the stream.
			if (bytes > StreamReadCutoffPoint)
			{
				return false;
			}

			FillBuffer();
			return true;
		}

		// Uses the Stream to fill the buffer.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FillBuffer()
		{
			if (_position == _buffer.Length)
			{
				// if we're at the end of the buffer, we don't need to copy data from the front
				// to the back, we can just read in the stream into the buffer
				var bytesRead = Stream.Read(_buffer.Span);
				_buffer = _buffer.Slice(0, bytesRead);
			}
			else
			{
				// copy data from the front of the buffer to the back
				var front = _buffer.Slice(_position);
				front.CopyTo(_buffer);
				_position = 0;

				// read in data from the stream
				var bytesRead = Stream.Read(_buffer.Slice(front.Length).Span);
				_buffer = _buffer.Slice(0, front.Length + bytesRead);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			_position += bytes;
		}
	}
}