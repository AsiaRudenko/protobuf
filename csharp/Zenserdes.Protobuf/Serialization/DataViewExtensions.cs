using System;

#nullable enable

namespace Zenserdes.Protobuf.Serialization
{
	/// <summary>
	/// Methods on <see cref="IDataView"/>s that are implemented as extension methods
	/// and not default interface methods for performance.
	/// </summary>
	public static class DataViewExtensions
	{
		// In these extension methods, they only apply to structs, and they take in the
		// structs as `ref`s (creating pointers).
		//
		// This is to optimize the most common case - the two default data views are
		// the StreamView, and the MemoryView. If a struct is at least as big as two references,
		// it would need to get saved to the stack so that the caller code can use the
		// data without spilling the registers. This is avoided by shadow copying the
		// struct to the stack, and then getting a pointer to it and passing it to the
		// caller function (and all of this is masked to the developer). Because it is
		// possible for the developer to modify the struct, the entire struct must be
		// copied before a pointer is gotten to it.
		//
		// In SpanView, MemoryView, and StreamView's cases, they are at or above the cutoff
		// limit, meaning that this shadow copying is performed. We can avoid the copy
		// by having the extension methods explicitly take in a reference to the structs.

		public static bool TryReadVarint32<TDataView>(this ref TDataView dataView, out int result)
			where TDataView : struct, IDataView
		{
			var bytes = dataView.ReadBytes(5);
			var dataResult = DataDecoder.TryReadVarint32(bytes);

			if (dataResult.BytesRead == 0)
			{
				result = default;
				return false;
			}

			dataView.Advance(dataResult.BytesRead);
			result = dataResult.Value;
			return true;
		}

		public static bool TryReadVarint64<TDataView>(this ref TDataView dataView, out long result)
			where TDataView : struct, IDataView
		{
			var bytes = dataView.ReadBytes(10);
			var dataResult = DataDecoder.TryReadVarint64(bytes);

			if (dataResult.BytesRead == 0)
			{
				result = default;
				return false;
			}

			dataView.Advance(dataResult.BytesRead);
			result = dataResult.Value;
			return true;
		}

		public static bool TryReadZigZag32<TDataView>(this ref TDataView dataView, out int result)
			where TDataView : struct, IDataView
		{
			throw new NotImplementedException();
		}

		public static bool TryReadZigZag64<TDataView>(this ref TDataView dataView, out long result)
			where TDataView : struct, IDataView
		{
			throw new NotImplementedException();
		}
	}
}