using System;
using System.Collections;
using YamlDotNet.Core;
using YamlDotNet.Serialization.ObjectFactories;

namespace YamlDotNet.Serialization.NodeDeserializers;

internal sealed class StaticArrayNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
{
	private sealed class ArrayList : IList, ICollection, IEnumerable
	{
		private object?[] data;

		public bool IsFixedSize => false;

		public bool IsReadOnly => false;

		public object? this[int index]
		{
			get
			{
				return data[index];
			}
			set
			{
				data[index] = value;
			}
		}

		public int Count { get; private set; }

		public bool IsSynchronized => false;

		public object SyncRoot => data;

		public ArrayList()
		{
			Clear();
		}

		public int Add(object? value)
		{
			if (Count == data.Length)
			{
				Array.Resize(ref data, data.Length * 2);
			}
			data[Count] = value;
			return Count++;
		}

		public void Clear()
		{
			data = new object[10];
			Count = 0;
		}

		bool IList.Contains(object? value)
		{
			throw new NotSupportedException();
		}

		int IList.IndexOf(object? value)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object? value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object? value)
		{
			throw new NotSupportedException();
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void CopyTo(Array array, int index)
		{
			Array.Copy(data, 0, array, index, Count);
		}

		public IEnumerator GetEnumerator()
		{
			int i = 0;
			while (i < Count)
			{
				yield return data[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	private readonly StaticObjectFactory factory;

	public StaticArrayNodeDeserializer(StaticObjectFactory factory)
	{
		this.factory = factory ?? throw new ArgumentNullException("factory");
	}

	public bool Deserialize(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser, Type, object?> nestedObjectDeserializer, out object? value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer rootDeserializer)
	{
		if (!factory.IsArray(expectedType))
		{
			value = false;
			return false;
		}
		Type valueType = factory.GetValueType(expectedType);
		ArrayList arrayList = new ArrayList();
		StaticCollectionNodeDeserializer.DeserializeHelper(valueType, parser, nestedObjectDeserializer, arrayList, factory);
		Array array = factory.CreateArray(expectedType, arrayList.Count);
		arrayList.CopyTo(array, 0);
		value = array;
		return true;
	}
}
