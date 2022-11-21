using System;
using System.Collections.Generic;
using System.Linq;

namespace Crimson.Core.Utils
{
	public static class ListUtils
	{
		private static Random _rnd = new Random();

		public static T PickRandom<T>(this IList<T> list)
		{
			return list[_rnd.Next(list.Count)];
		}

		public static IList<T> PickRandom<T>(this IList<T> source, int count)
		{
			return source.OrderBy(x => _rnd.Next(source.Count)).Take(count).ToList();
		}

		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = _rnd.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}

		public static void Shuffle<T>(this T[] items)
		{
			int n = items.Length;
			while (n > 1)
			{
				n--;
				int k = _rnd.Next(n + 1);
				T value = items[k];
				items[k] = items[n];
				items[n] = value;
			}
		}

		public static bool SequenceEqual<T>(this IList<T> firstList, IList<T> secondList)
		{
			var cnt = new Dictionary<T, int>();

			foreach (T s in firstList)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]++;
				}
				else
				{
					cnt.Add(s, 1);
				}
			}
			foreach (T s in secondList)
			{
				if (cnt.ContainsKey(s))
				{
					cnt[s]--;
				}
				else
				{
					return false;
				}
			}
			return cnt.Values.All(c => c == 0);
		}

		public static bool ContainsAllItems<T>(this IList<T> firstList, IList<T> secondList)
		{
			return !secondList.Except(firstList).Any();
		}

		public static bool ContainsItems<T>(this IList<T> firstList, IList<T> secondList)
		{
			return firstList.Any(secondList.Contains);
		}
	}
}