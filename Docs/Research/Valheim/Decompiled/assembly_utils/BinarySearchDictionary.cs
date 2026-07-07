using System;
using System.Collections;
using System.Collections.Generic;

public class BinarySearchDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IDictionary<TKey, TValue>, ICloneable where TKey : IComparable<TKey>
{
	private TKey[] m_keys;

	private TValue[] m_values;

	private ushort m_length;

	public TValue this[TKey key]
	{
		get
		{
			if (m_length <= 0)
			{
				throw new KeyNotFoundException("This BinarySearchDictionary is empty!");
			}
			bool exactMatch;
			int num = BinaryFindKeyIndex(key, out exactMatch);
			if (!exactMatch)
			{
				throw new KeyNotFoundException("Key could not be found in this BinarySearchDictionary!");
			}
			return m_values[num];
		}
		set
		{
			if (m_length <= 0)
			{
				GuaranteeCapacity();
				m_keys[0] = key;
				m_values[0] = value;
				m_length++;
				return;
			}
			bool exactMatch;
			int num = BinaryFindKeyIndex(key, out exactMatch);
			if (exactMatch)
			{
				m_keys[num] = key;
				m_values[num] = value;
				return;
			}
			GuaranteeCapacity();
			if (m_length - num > 0)
			{
				Array.Copy(m_keys, num, m_keys, num + 1, m_length - num);
				Array.Copy(m_values, num, m_values, num + 1, m_length - num);
			}
			m_keys[num] = key;
			m_values[num] = value;
			m_length++;
		}
	}

	public ICollection<TKey> Keys => m_keys;

	public ICollection<TValue> Values => m_values;

	public int Count => m_length;

	public bool IsReadOnly => false;

	public int Capacity
	{
		get
		{
			if (m_keys != null)
			{
				return m_keys.Length;
			}
			return 0;
		}
		set
		{
			if (Capacity < value)
			{
				if (m_keys == null)
				{
					m_keys = new TKey[value];
					m_values = new TValue[value];
					return;
				}
				TKey[] keys = m_keys;
				TValue[] values = m_values;
				m_keys = new TKey[value];
				m_values = new TValue[value];
				Array.Copy(keys, m_keys, m_length);
				Array.Copy(values, m_values, m_length);
			}
		}
	}

	public object Clone()
	{
		return MemberwiseClone();
	}

	public void Add(TKey key, TValue value)
	{
		GuaranteeCapacity();
		bool exactMatch;
		int num = BinaryFindKeyIndex(key, out exactMatch);
		if (exactMatch)
		{
			throw new ArgumentException("Duplicate keys are not allowed!");
		}
		if (m_length - num > 0)
		{
			Array.Copy(m_keys, num, m_keys, num + 1, m_length - num);
			Array.Copy(m_values, num, m_values, num + 1, m_length - num);
		}
		m_keys[num] = key;
		m_values[num] = value;
		m_length++;
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		m_length = 0;
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		if (m_length <= 0)
		{
			return false;
		}
		bool exactMatch;
		int num = BinaryFindKeyIndex(item.Key, out exactMatch);
		if (exactMatch)
		{
			return Compare(m_values[num], item.Value);
		}
		return false;
	}

	public bool ContainsKey(TKey key)
	{
		if (m_length <= 0)
		{
			return false;
		}
		BinaryFindKeyIndex(key, out var exactMatch);
		return exactMatch;
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (m_length > 0)
		{
			for (int i = 0; i < m_length; i++)
			{
				array[i] = new KeyValuePair<TKey, TValue>(m_keys[i], m_values[i]);
			}
		}
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		for (int i = 0; i < m_length; i++)
		{
			yield return new KeyValuePair<TKey, TValue>(m_keys[i], m_values[i]);
		}
	}

	public bool Remove(TKey key)
	{
		if (m_length <= 0)
		{
			return false;
		}
		bool exactMatch;
		int num = BinaryFindKeyIndex(key, out exactMatch);
		if (!exactMatch)
		{
			return false;
		}
		if (m_length - (num + 1) > 0)
		{
			Array.Copy(m_keys, num + 1, m_keys, num, m_length - (num + 1));
			Array.Copy(m_values, num + 1, m_values, num, m_length - (num + 1));
		}
		m_length--;
		return true;
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (m_length <= 0)
		{
			return false;
		}
		bool exactMatch;
		int num = BinaryFindKeyIndex(item.Key, out exactMatch);
		if (!exactMatch || !Compare(m_values[num], item.Value))
		{
			return false;
		}
		if (m_length - (num + 1) > 0)
		{
			Array.Copy(m_keys, num + 1, m_keys, num, m_length - (num + 1));
			Array.Copy(m_values, num + 1, m_values, num, m_length - (num + 1));
		}
		m_length--;
		return true;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (m_length <= 0)
		{
			value = default(TValue);
			return false;
		}
		bool exactMatch;
		int num = BinaryFindKeyIndex(key, out exactMatch);
		if (exactMatch)
		{
			value = m_values[num];
			return true;
		}
		value = default(TValue);
		return false;
	}

	public TValue GetValueOrDefault(TKey key, TValue defaultValue)
	{
		bool exactMatch;
		int num = BinaryFindKeyIndex(key, out exactMatch);
		if (exactMatch)
		{
			return m_values[num];
		}
		return defaultValue;
	}

	public bool SetValue(TKey key, TValue value)
	{
		bool exactMatch;
		int num = BinaryFindKeyIndex(key, out exactMatch);
		if (exactMatch)
		{
			if (m_values[num].Equals(value))
			{
				return false;
			}
			m_values[num] = value;
			return true;
		}
		GuaranteeCapacity();
		if (m_length - num > 0)
		{
			Array.Copy(m_keys, num, m_keys, num + 1, m_length - num);
			Array.Copy(m_values, num, m_values, num + 1, m_length - num);
		}
		m_keys[num] = key;
		m_values[num] = value;
		m_length++;
		return true;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Reserve(int size)
	{
		if (Capacity <= size)
		{
			Capacity = size;
		}
	}

	private void GuaranteeCapacity()
	{
		if (Capacity <= m_length)
		{
			if (Capacity == 0)
			{
				Capacity = 1;
			}
			else
			{
				Capacity += 2;
			}
		}
	}

	private int BinaryFindKeyIndex(TKey key, out bool exactMatch)
	{
		if (m_length <= 0)
		{
			exactMatch = false;
			return 0;
		}
		int num = 0;
		int num2 = m_length - 1;
		while (num < num2)
		{
			int num3 = (num + num2) / 2;
			int num4 = key.CompareTo(m_keys[num3]);
			if (num4 == 0)
			{
				exactMatch = true;
				return num3;
			}
			if (num4 < 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		int num5 = key.CompareTo(m_keys[num]);
		exactMatch = num5 == 0;
		if (num5 > 0)
		{
			return num + 1;
		}
		return num;
	}

	private bool Compare<T>(T lhs, T rhs)
	{
		return EqualityComparer<T>.Default.Equals(lhs, rhs);
	}
}
