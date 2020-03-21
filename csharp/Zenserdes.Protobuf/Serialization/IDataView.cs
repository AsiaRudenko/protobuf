using System;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	// TODO: perhaps the amount of data copying is not ideal. for performance, it'd
	// be faster if we could just return the data that's in the buffer.

	/// <summary>
	/// Represents the blanket capabilities of a view that can read data.
	/// </summary>
	public interface IDataView
	{
		// TODO: figure out api
		ReadOnlySpan<byte> ReadBytes(int bytes);

		ReadOnlyMemory<byte> ReadBytesToMemory(int bytes);

		/// <summary>
		/// Reads as many bytes as possible into the target.
		/// </summary>
		/// <param name="target">The target for the bytes to end up in.</param>
		/// <returns>The amount of bytes read.</returns>
		int ReadBytes(Span<byte> target);

		/// <summary>
		/// Reads as many bytes as possible into the target.
		/// </summary>
		/// <param name="target">The target for the bytes to end up in.</param>
		/// <returns>The amount of bytes read.</returns>
		int ReadBytesToMemory(Memory<byte> target);

		/// <summary>
		/// Advances the reader by the amount specified.
		/// <para>
		/// It is preferred for the caller to manually specify if they want to advance,
		/// so the caller can make a guess at how much data they're going to read, and
		/// then proceed to advance based on how many bytes they used.
		/// </para>
		/// </summary>
		/// <param name="bytes">The bytes that were advanced.</param>
		void Advance(int bytes);
	}
}