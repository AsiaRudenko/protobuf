using System.Collections.Generic;

#nullable enable

namespace Zenserdes.Protobuf.ZenGen
{
	public static class EnumerableExtensions
	{
		// Used to keep code tidy that checks if an item is the last item. This makes writing
		// lines inbetween elements cleaner
		public static IEnumerable<(T, bool)> FlagLast<T>(this IEnumerable<T> enumerable)
		{
			bool didSet = false;
			T last = default;

			foreach (var item in enumerable)
			{
				if (!didSet)
				{
					last = item;
					didSet = true;
				}
				else
				{
					yield return (last, false);
					last = item;
				}
			}

			if (didSet)
			{
				yield return (last, true);
			}
		}
	}
}