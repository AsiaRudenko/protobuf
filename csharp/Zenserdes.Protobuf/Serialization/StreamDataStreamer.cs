using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public ref struct StreamDataStreamer<TBufferWriter>
		where TBufferWriter : IBufferWriter<byte>
	{
		public Stream Stream;
		public TBufferWriter BufferWriter;

		public const int BufferSize = 4096;
		public Span<byte> Buffer;
		public int BufferPosition;

		public StreamDataStreamer(Stream stream, TBufferWriter bufferWriter, Span<byte> buffer)
		{
			Stream = stream;
			BufferWriter = bufferWriter;
			Buffer = buffer;
			BufferPosition = 0;

			// this limitation is here because knowing that the initial buffer size is 4096
			// is useful while doing buffer stuff. of course this can be modified in source
			// to support larger buffers, but i don't really think that matters.
			Debug.Assert(Buffer.Length == BufferSize, $"Buffer must be {BufferSize} bytes.");

			FillBuffer();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe bool Next(ref byte data)
		{
			if (!CanReadAtLeastNBytes(1))
			{
				if (!FitNBytesIntoBuffer(1))
				{
					return false;
				}
			}

			data = Buffer[BufferPosition++];
			return true;
		}


		public ReadOnlySpan<byte> MaybeReadWorkaround(int max)
			=> Buffer.Slice(BufferPosition, Math.Min(Buffer.Length - BufferPosition, max));

		/// <summary>
		/// Only used for varint decoding. This isn't very useful in any other situation,
		/// because there is no way to tell how many bytes were copied.
		/// </summary>
		/// <param name="target"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void MaybeRead(Span<byte> target)
		{
			Buffer.Slice(BufferPosition).CopyTo(target);
		}

		/// <summary>
		/// The caller must verify that advancing the buffer does not exceed the buffer length.
		/// How? No idea. Don't touch this.
		/// </summary>
		/// <param name="bytes"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Advance(int bytes)
		{
			BufferPosition += bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool ReadPermanent(int bytes, out ReadOnlyMemory<byte> memory)
		{
			var target = BufferWriter.GetMemory(bytes).Slice(0, bytes);

			if (bytes >= Buffer.Length - BufferPosition)
			{
				// don't have enough buffer space to copy from the buffer to the target

				if (bytes < Buffer.Length)
				{
					// if we're reading less bytes than are in the buffer, we can just read in
					// the buffer and then read the buffer to the target

					ShiftBuffer(Buffer.Length - BufferPosition);

					// enough data in buffer to copy to target
					ReadFromBuffer(bytes, target.Span);

					// exiting out of nested loops (if statements in this case) is a perfectly
					// valid use of goto.
					goto EXIT_IF_STATEMENTS;
				}

				// let's just copy the remaining amount of the buffer to the target
				// and read in the rest from the stream

				Buffer.Slice(BufferPosition).CopyTo(target.Span);
				var bytesWritten = Buffer.Length - BufferPosition;

				var targetRemaining = target.Span.Slice(bytesWritten);
				var bytesRead = Stream.Read(targetRemaining);

				if (bytesRead != targetRemaining.Length)
				{
					memory = target;
					return false;
				}

				FillBuffer();
			}
			else
			{
				// enough data in buffer to copy to target
				ReadFromBuffer(bytes, target.Span);
			}

			EXIT_IF_STATEMENTS:

			memory = target;
			BufferWriter.Advance(bytes);
			return true;
		}

		/// <summary>
		/// Reads the entire stream into the buffer, resizing the buffer as necessary.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void FillBuffer()
		{
			BufferPosition = 0;
			var size = Stream.Read(Buffer);
			Buffer = Buffer.Slice(0, size);
		}

		/// <summary>
		/// Takes <paramref name="bytes"/> bytes from the front of the buffer, moves them
		/// to the back of the buffer, and then reads from the stream to read in the remaining
		/// bytes.
		/// </summary>
		/// <param name="bytes"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ShiftBuffer(int bytes)
		{
			var front = Buffer.Slice(Buffer.Length - bytes, bytes);
			var back = Buffer.Slice(0, bytes);
			var negativeBack = Buffer.Slice(bytes);

			var bytesRead = Stream.Read(negativeBack);
			Buffer = Buffer.Slice(0, bytesRead + bytes);

			BufferPosition -= bytes;
		}

		/// <summary>
		/// Reads <paramref name="bytes"/> bytes from the buffer and copies them into
		/// the <paramref name="target"/>. This method expects the caller to check the
		/// parameters they pass in.
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="target"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ReadFromBuffer(int bytes, Span<byte> target)
		{
			var bufferSlice = Buffer.Slice(BufferPosition, bytes);
			bufferSlice.CopyTo(target);
			BufferPosition += bytes;
		}

		/// <summary>
		/// Checks if there are <paramref name="n"/> bytes that can be read.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool CanReadAtLeastNBytes(int n)
		{
			return BufferPosition + n < Buffer.Length;
		}

		/// <summary>
		/// Tries to fit <paramref name="n"/> bytes into the buffer by reading it from
		/// the stream. This expects the caller to know that at least <paramref name="n"/>
		/// bytes *can't* be read from the buffer.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool FitNBytesIntoBuffer(int n)
		{
			if (Buffer.Length != BufferSize)
			{
				// the caller has checked that we can't fit `n` bytes into the stream. we are
				// guarenteed to be unable to read anymore in.
				return false;
			}

			ShiftBuffer(Buffer.Length - BufferPosition);
			return CanReadAtLeastNBytes(n);
		}
	}
}