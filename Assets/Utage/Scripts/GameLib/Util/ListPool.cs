// UTAGE: Unity Text Adventure Game Engine (c) Ryohei Tokimurausing UnityEngine;
using System.Collections.Generic;

namespace Utage
{
	internal static class ListPool<T>
	{
		// Object pool to avoid allocations.
		private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

		public static List<T> Get()
		{
			return s_ListPool.Get();
		}

		public static void Release(List<T> toRelease)
		{
			s_ListPool.Release(toRelease);
		}
	}

}