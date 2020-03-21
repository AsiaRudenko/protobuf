using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

#nullable enable

namespace Zenserdes.Protobuf
{
	/// <summary>
	/// Public API for Zenserdes.Protobufm which contains all important entrypoint
	/// methods for using the library.
	/// <para>
	/// All methods are thoroughly documented and explained. ideally, you should never
	/// have to reach farther beyond this single class.
	/// </para>
	/// </summary>
	public static class ZenserdesProtobuf
	{
		[ThreadStatic]
		private static ArrayBufferWriter<byte>? _bufferWriter = null;

		/// <summary>
		/// Provides thread local access to a singleton instance of an
		/// <see cref="ArrayBufferWriter{T}"/>, available for use for all threads.
		/// <para>
		/// This static instance is used when the user doesn't pass in their own
		/// instance. In turn, it reduces the amount of allocations performed.
		/// </para>
		/// <para>
		/// Warning: Avoid using this property. This property automatically
		/// initializes the backing <see cref="ArrayBufferWriter{T}"/> if it is
		/// null, and clears the buffer on every property access. This is because
		/// every thread must initialize thread local data on their own
		/// (see <see cref="ThreadStaticAttribute"/>), and the buffer is cleared
		/// to prevent extraneous growth of the buffer, which would lead to memory
		/// issues.
		/// </para>
		/// </summary>
		public static ArrayBufferWriter<byte> CachedBufferWriter
		{
			get
			{
				if (_bufferWriter == null)
				{
					_bufferWriter = new ArrayBufferWriter<byte>();
				}

				// TODO: investigate using reflection to set _index to 0, to
				// prevent the data from being cleared.
				_bufferWriter.Clear();
				return _bufferWriter;
			}
			set => _bufferWriter = value;
		}

		// TODO: for documentation on `message`s, include information aobut using
		// whatever zenserdes tool there is for generating the messages.

		/// <summary>
		/// Serializes the given message into bytes.
		/// <para>
		/// This method is provided as a comfort method. It is recommended to
		/// use an overload to be more specific with your resources, such as
		/// <see cref="Serialize{TMessage, TBufferWriter}(TMessage, TBufferWriter)"/>.
		/// </para>
		/// </summary>
		/// <typeparam name="TMessage">The type of message to serialize.</typeparam>
		/// <param name="message">The message to serialize.</param>
		/// <param name="longRetention">If the data returned to you should be persistent
		/// after multiple de/serialization calls. If this is <c>false</c>, then any data
		/// you use or receive after a second call to any public API method to Zenserdes.Protobuf
		/// will make the initial data you received stale.
		/// <para>
		/// An example of when it'd be perfect to disable long retention is during benchmarks,
		/// as on each iteration of the benchmark the data used is discareded and not needed,
		/// meaning that it won't matter if the previously handled data is stale. If many
		/// operations are being done in succession where the previous data is discarded
		/// (such that it won't matter if the old data is stale), disabling long retention may
		/// yield huge performance boosts, by preventing an <see cref="ArrayBufferWriter{T}"/>
		/// allocation as well as preventing array allocations since they will be reused.
		/// </para></param>
		/// <returns>The serialized protobuf message.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory<byte> Serialize<TMessage>
		(
			[NotNull] TMessage message,
			bool longRetention = true
		)
			where TMessage : IZenserdesProtobufMessage
			=> Serialize(message, BufferForRetention(longRetention, sizeHint: message.SizeHint));

		/// <summary>
		/// Serializes the given message into bytes.
		/// </summary>
		/// <typeparam name="TMessage">The type message to serialize.</typeparam>
		/// <typeparam name="TBufferWriter">The type of buffer writer to use.</typeparam>
		/// <param name="message">The message to serialize.</param>
		/// <param name="bufferWriter">The buffer writer to use.
		/// If none is specified, <see cref="CachedBufferWriter"/> is used.</param>
		/// <returns>The serialized protobuf message.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Memory<byte> Serialize<TMessage, TBufferWriter>
		(
			[NotNull] TMessage message,
			[NotNull] TBufferWriter bufferWriter
		)
			where TMessage : IZenserdesProtobufMessage
			where TBufferWriter : IBufferWriter<byte>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes the given message into a span of bytes.
		/// <para>
		/// This method will throw exceptions if the target buffer is not large enough.
		/// </para>
		/// </summary>
		/// <typeparam name="TMessage">The type of message to serialize.</typeparam>
		/// <param name="message">The message to serialize.</param>
		/// <param name="target">The target bytes to serialize into.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize<TMessage>
		(
			[NotNull] TMessage message,
			Span<byte> target
		)
			where TMessage : IZenserdesProtobufMessage
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Serializes the given message into a <see cref="Stream"/>.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to serialize.</typeparam>
		/// <param name="message">The message to serialize.</param>
		/// <param name="target">The target stream to serialize into.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize<TMessage>
		(
			[NotNull] TMessage message,
			[NotNull] Stream target
		)
			where TMessage : IZenserdesProtobufMessage
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message into its representation from a source of bytes.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <param name="source">The source of bytes to deserialize from.</param>
		/// <returns>The message, if successful.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TMessage Deserialize<TMessage>
		(
			// the reason we are not accepting spans is because you can't go from
			// a span to a memory, and the TMessage might have fields that require
			// ROM<byte>s.
			ReadOnlyMemory<byte> source
		)
			where TMessage : IZenserdesProtobufMessage
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message from a span to an instance.
		/// <para>
		/// Warning: this method may cause extra allocations. If the <typeparamref name="TMessage"/>
		/// specified has any fields which accept <see cref="ReadOnlyMemory{T}"/>, then
		/// you are guarenteed to incur additional allocations. You are also guarenteed
		/// additional allocations if the received protobuf has fields not defined on
		/// the message. Please read the documentation of <paramref name="longRetention"/>
		/// to see if you are eligable to turn off the flag,
		/// which would yield performance gains.
		/// </para>
		/// <para>
		/// For the reason(s) stated above, please prefer the alternative <see cref="Deserialize{TMessage}(ReadOnlyMemory{byte})"/>
		/// method whenever possible.
		/// </para>
		/// </summary>
		/// <typeparam name="TMessage">The type of message to serialize.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="longRetention">If the data returned to you should be persistent
		/// after multiple de/serialization calls. If this is <c>false</c>, then any data
		/// you use or receive after a second call to any public API method to Zenserdes.Protobuf
		/// will make the initial data you received stale.
		/// <para>
		/// An example of when it'd be perfect to disable long retention is during benchmarks,
		/// as on each iteration of the benchmark the data used is discareded and not needed,
		/// meaning that it won't matter if the previously handled data is stale. If many
		/// operations are being done in succession where the previous data is discarded
		/// (such that it won't matter if the old data is stale), disabling long retention may
		/// yield huge performance boosts, by preventing an <see cref="ArrayBufferWriter{T}"/>
		/// allocation as well as preventing array allocations since they will be reused.
		/// </para></param>
		/// <returns>The message, if successful.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TMessage Deserialize<TMessage>
		(
			ReadOnlySpan<byte> source,
			bool longRetention = true
		)
			where TMessage : IZenserdesProtobufMessage
			=> Deserialize<TMessage, ArrayBufferWriter<byte>>
			(
				source,
				BufferForRetention(longRetention)
			);

		/// <summary>
		/// Deserializes a message from a span to an instance.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <typeparam name="TBufferWriter">The type of buffer writer to use.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="bufferWriter">The instance of the buffer writer to use.</param>
		/// <returns>The message, if successful.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TMessage Deserialize<TMessage, TBufferWriter>
		(
			ReadOnlySpan<byte> source,
			[NotNull] TBufferWriter bufferWriter
		)
			where TMessage : IZenserdesProtobufMessage
			where TBufferWriter : IBufferWriter<byte>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message from a stream source to an instance. This will consume
		/// the entire stream.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="longRetention">If the data returned to you should be persistent
		/// after multiple de/serialization calls. If this is <c>false</c>, then any data
		/// you use or receive after a second call to any public API method to Zenserdes.Protobuf
		/// will make the initial data you received stale.
		/// <para>
		/// An example of when it'd be perfect to disable long retention is during benchmarks,
		/// as on each iteration of the benchmark the data used is discareded and not needed,
		/// meaning that it won't matter if the previously handled data is stale. If many
		/// operations are being done in succession where the previous data is discarded
		/// (such that it won't matter if the old data is stale), disabling long retention may
		/// yield huge performance boosts, by preventing an <see cref="ArrayBufferWriter{T}"/>
		/// allocation as well as preventing array allocations since they will be reused.
		/// </para></param>
		/// <returns>The message, if successful.</returns>
		public static TMessage Deserialize<TMessage>
		(
			[NotNull] Stream source,
			bool longRetention = true
		)
			where TMessage : IZenserdesProtobufMessage
			=> Deserialize<TMessage, ArrayBufferWriter<byte>>
			(
				source,
				BufferForRetention(longRetention)
			);

		/// <summary>
		/// Deserializes a message from a stream source to an instance. This will consume
		/// the entire stream.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <typeparam name="TBufferWriter">The type of buffer writer to use.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="bufferWriter">The buffer writer to use.</param>
		/// <returns>The message, if successful.</returns>
		public static TMessage Deserialize<TMessage, TBufferWriter>
		(
			[NotNull] Stream source,
			[NotNull] TBufferWriter bufferWriter
		)
			where TMessage : IZenserdesProtobufMessage
			where TBufferWriter : IBufferWriter<byte>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message into its representation from a source of bytes.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <param name="source">The source of bytes to deserialize from.</param>
		/// <param name="message">The message, if the payload was deserialized.</param>
		/// <returns>True if the payload was deserialized, false if it wasn't.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryDeserialize<TMessage>
		(
			// the reason we are not accepting spans is because you can't go from
			// a span to a memory, and the TMessage might have fields that require
			// ROM<byte>s.
			ReadOnlyMemory<byte> source,
			[MaybeNullWhen(false)] out TMessage message
		)
			where TMessage : IZenserdesProtobufMessage
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message from a span to an instance.
		/// <para>
		/// Warning: this method may cause extra allocations. If the <typeparamref name="TMessage"/>
		/// specified has any fields which accept <see cref="ReadOnlyMemory{T}"/>, then
		/// you are guarenteed to incur additional allocations. You are also guarenteed
		/// additional allocations if the received protobuf has fields not defined on
		/// the message. Please read the documentation of <paramref name="longRetention"/>
		/// to see if you are eligable to turn off the flag,
		/// which would yield performance gains.
		/// </para>
		/// <para>
		/// For the reason(s) stated above, please prefer the alternative <see cref="Deserialize{TMessage}(ReadOnlyMemory{byte})"/>
		/// method whenever possible.
		/// </para>
		/// </summary>
		/// <typeparam name="TMessage">The type of message to serialize.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="message">The message, if the payload was deserialized.</param>
		/// <param name="longRetention">If the data returned to you should be persistent
		/// after multiple de/serialization calls. If this is <c>false</c>, then any data
		/// you use or receive after a second call to any public API method to Zenserdes.Protobuf
		/// will make the initial data you received stale.
		/// <para>
		/// An example of when it'd be perfect to disable long retention is during benchmarks,
		/// as on each iteration of the benchmark the data used is discareded and not needed,
		/// meaning that it won't matter if the previously handled data is stale. If many
		/// operations are being done in succession where the previous data is discarded
		/// (such that it won't matter if the old data is stale), disabling long retention may
		/// yield huge performance boosts, by preventing an <see cref="ArrayBufferWriter{T}"/>
		/// allocation as well as preventing array allocations since they will be reused.
		/// </para></param>
		/// <returns>True if the payload was deserialized, false if it wasn't.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TruDeserialize<TMessage>
		(
			ReadOnlySpan<byte> source,
			[MaybeNullWhen(false)] out TMessage message,
			bool longRetention = true
		)
			where TMessage : IZenserdesProtobufMessage
			=> TryDeserialize<TMessage, ArrayBufferWriter<byte>>
			(
				source,
				BufferForRetention(longRetention),
				out message
			);

		/// <summary>
		/// Deserializes a message from a span to an instance.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <typeparam name="TBufferWriter">The type of buffer writer to use.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="bufferWriter">The instance of the buffer writer to use.</param>
		/// <param name="message">The message, if the payload was deserialized.</param>
		/// <returns>True if the payload was deserialized, false if it wasn't.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryDeserialize<TMessage, TBufferWriter>
		(
			ReadOnlySpan<byte> source,
			[NotNull] TBufferWriter bufferWriter,
			[MaybeNullWhen(false)] out TMessage message
		)
			where TMessage : IZenserdesProtobufMessage
			where TBufferWriter : IBufferWriter<byte>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Deserializes a message from a stream source to an instance. This will consume
		/// the entire stream.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="message">The message, if the payload was deserialized.</param>
		/// <param name="longRetention">If the data returned to you should be persistent
		/// after multiple de/serialization calls. If this is <c>false</c>, then any data
		/// you use or receive after a second call to any public API method to Zenserdes.Protobuf
		/// will make the initial data you received stale.
		/// <para>
		/// An example of when it'd be perfect to disable long retention is during benchmarks,
		/// as on each iteration of the benchmark the data used is discareded and not needed,
		/// meaning that it won't matter if the previously handled data is stale. If many
		/// operations are being done in succession where the previous data is discarded
		/// (such that it won't matter if the old data is stale), disabling long retention may
		/// yield huge performance boosts, by preventing an <see cref="ArrayBufferWriter{T}"/>
		/// allocation as well as preventing array allocations since they will be reused.
		/// </para></param>
		/// <returns>True if the payload was deserialized, false if it wasn't.</returns>
		public static bool TryDeserialize<TMessage>
		(
			[NotNull] Stream source,
			[MaybeNullWhen(false)] out TMessage message,
			bool longRetention = true
		)
			where TMessage : IZenserdesProtobufMessage
			=> TryDeserialize<TMessage, ArrayBufferWriter<byte>>
			(
				source,
				BufferForRetention(longRetention),
				out message
			);

		/// <summary>
		/// Deserializes a message from a stream source to an instance. This will consume
		/// the entire stream.
		/// </summary>
		/// <typeparam name="TMessage">The type of message to deserialize.</typeparam>
		/// <typeparam name="TBufferWriter">The type of buffer writer to use.</typeparam>
		/// <param name="source">The source of the protobuf data.</param>
		/// <param name="bufferWriter">The buffer writer to use.</param>
		/// <param name="message">The message, if the payload was deserialized.</param>
		/// <returns>True if the payload was deserialized, false if it wasn't.</returns>
		public static bool TryDeserialize<TMessage, TBufferWriter>
		(
			[NotNull] Stream source,
			[NotNull] TBufferWriter bufferWriter,
			[MaybeNullWhen(false)] out TMessage message
		)
			where TMessage : IZenserdesProtobufMessage
			where TBufferWriter : IBufferWriter<byte>
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Returns the ideal buffer for the user based on their retention requirements.
		/// <para>
		/// Long retention of data is when the user expects their previously serialized
		/// or deserialized data to be the same on another call to serialize/deserialize on
		/// the same thread.
		/// </para>
		/// <para>
		/// If the user explicitly opts out, we can save an allocation and let the user
		/// use the <see cref="CachedBufferWriter"/> instead.
		/// </para>
		/// </summary>
		/// <param name="longRetention">Whether or not the user desires long retention of data.</param>
		/// <param name="sizeHint">The size to initialize the buffer to. Set to '256' by default,
		/// as that's what the DefaultInitialBufferSize is (check source.dot.net)</param>
		/// <returns>The ideal buffer to hand the user based on their requirements.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static ArrayBufferWriter<byte> BufferForRetention(bool longRetention, int sizeHint = 256)
			=> longRetention ? new ArrayBufferWriter<byte>(sizeHint) : CachedBufferWriter;
	}
}