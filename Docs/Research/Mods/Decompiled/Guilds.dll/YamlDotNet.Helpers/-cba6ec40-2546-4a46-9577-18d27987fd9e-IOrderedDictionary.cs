using System.Collections;
using System.Collections.Generic;

namespace YamlDotNet.Helpers;

internal interface _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIOrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable where TKey : notnull
{
	KeyValuePair<TKey, TValue> this[int index] { get; set; }

	void Insert(int index, TKey key, TValue value);

	void RemoveAt(int index);
}
