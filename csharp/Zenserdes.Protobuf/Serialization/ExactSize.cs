using System.Runtime.CompilerServices;

namespace Zenserdes.Protobuf.Serialization
{
	public static class ExactSize
	{
		// TODO: check the implementation of these compared to how .NET does it, see:
		// https://source.dot.net/#PresentationFramework/System/Windows/Markup/BamlBinaryWriter.cs,33

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int VarintSize(uint value)
		{
			if (value < 0b1_0000000u)
			{
				return 1;
			}

			if (value < 0b1_0000000_0000000u)
			{
				return 2;
			}

			if (value < 0b1_0000000_0000000_0000000u)
			{
				return 3;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000u)
			{
				return 4;
			}

			return 5;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int VarintSize(ulong value)
		{
			if (value < 0b1_0000000uL)
			{
				return 1;
			}

			if (value < 0b1_0000000_0000000uL)
			{
				return 2;
			}

			if (value < 0b1_0000000_0000000_0000000uL)
			{
				return 3;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000uL)
			{
				return 4;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000_0000000uL)
			{
				return 5;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000_0000000_0000000uL)
			{
				return 6;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000_0000000_0000000_0000000uL)
			{
				return 7;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000_0000000_0000000_0000000_0000000uL)
			{
				return 8;
			}

			if (value < 0b1_0000000_0000000_0000000_0000000_0000000_0000000_0000000_0000000_0000000uL)
			{
				return 9;
			}

			return 10;
		}
	}
}