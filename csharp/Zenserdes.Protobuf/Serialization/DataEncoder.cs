using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public static class DataEncoder
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint EncodeZizZag32(int value)
			=> (uint)((value << 1) ^ (value >> 31));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong EncodeZizZag64(long value)
			=> (ulong)((value << 1) ^ (value >> 63));
	}
}