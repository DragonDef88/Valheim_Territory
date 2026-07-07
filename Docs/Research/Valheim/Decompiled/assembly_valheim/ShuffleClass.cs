using System;
using System.Collections.Generic;
using UnityEngine;

internal static class ShuffleClass
{
	private static Random rng = new Random();

	public static void Shuffle<T>(this IList<T> list, bool useUnityRandom = false)
	{
		int num = list.Count;
		while (num > 1)
		{
			num--;
			int index = (useUnityRandom ? Random.Range(0, num) : rng.Next(num + 1));
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}
}
