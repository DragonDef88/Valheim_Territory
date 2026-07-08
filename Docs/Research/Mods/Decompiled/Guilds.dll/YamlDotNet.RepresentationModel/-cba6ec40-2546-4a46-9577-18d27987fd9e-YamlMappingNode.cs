using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization;

namespace YamlDotNet.RepresentationModel;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, IEnumerable<KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>>, IEnumerable, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EOrderedDictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> children = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EOrderedDictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIOrderedDictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> Children => children;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle Style { get; set; }

	public override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType NodeType => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType.Mapping;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		Load(parser, state);
	}

	private void Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>();
		Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart, state);
		Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart.Style;
		bool flag = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd>(out @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode.ParseNode(parser, state);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode3 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode.ParseNode(parser, state);
			if (!children.TryAdd(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode3))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2.Start;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException(in start, in end, $"Duplicate key {_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2}");
			}
			flag = flag || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode3 is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode;
		}
		if (flag)
		{
			state.AddNodeWithUnresolvedAliases(this);
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode()
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(params KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>[] children)
		: this((IEnumerable<KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>>)children)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(IEnumerable<KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>> children)
	{
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			this.children.Add(child);
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(params _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode[] children)
		: this((IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>)children)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> children)
	{
		using IEnumerator<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> enumerator = children.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode current = enumerator.Current;
			if (!enumerator.MoveNext())
			{
				throw new ArgumentException("When constructing a mapping node with a sequence, the number of elements of the sequence must be even.");
			}
			Add(current, enumerator.Current);
		}
	}

	public void Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value)
	{
		children.Add(key, value);
	}

	public void Add(string key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value)
	{
		children.Add(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(key), value);
	}

	public void Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode key, string value)
	{
		children.Add(key, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(value));
	}

	public void Add(string key, string value)
	{
		children.Add(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(key), new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(value));
	}

	internal override void ResolveAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> dictionary = null;
		Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> dictionary2 = null;
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			if (child.Key is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();
				}
				dictionary.Add(child.Key, state.GetNode(child.Key.Anchor, child.Key.Start, child.Key.End));
			}
			if (child.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode)
			{
				if (dictionary2 == null)
				{
					dictionary2 = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();
				}
				dictionary2.Add(child.Key, state.GetNode(child.Value.Anchor, child.Value.Start, child.Value.End));
			}
		}
		if (dictionary2 != null)
		{
			foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> item in dictionary2)
			{
				children[item.Key] = item.Value;
			}
		}
		if (dictionary == null)
		{
			return;
		}
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> item2 in dictionary)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value = children[item2.Key];
			children.Remove(item2.Key);
			children.Add(item2.Value, value);
		}
	}

	internal override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state)
	{
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(base.Anchor, base.Tag, isImplicit: true, Style));
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			child.Key.Save(emitter, state);
			child.Value.Save(emitter, state);
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd());
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2) || !object.Equals(base.Tag, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2.Tag) || children.Count != _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2.children.Count)
		{
			return false;
		}
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2.children.TryGetValue(child.Key, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value) || !object.Equals(child.Value, value))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int num = base.GetHashCode();
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			num = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(num, child.Key);
			num = (child.Value.Anchor.IsEmpty ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(num, child.Value) : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(num, child.Value.Anchor));
		}
		return num;
	}

	internal override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> SafeAllNodes(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		level.Increment();
		yield return this;
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
		{
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode item in child.Key.SafeAllNodes(level))
			{
				yield return item;
			}
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode item2 in child.Value.SafeAllNodes(level))
			{
				yield return item2;
			}
		}
		level.Decrement();
	}

	internal override string ToString(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		if (!level.TryIncrement())
		{
			return "WARNING! INFINITE RECURSION!";
		}
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			builder.Append("{ ");
			foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in children)
			{
				if (builder.Length > 2)
				{
					builder.Append(", ");
				}
				builder.Append("{ ").Append(child.Key.ToString(level)).Append(", ")
					.Append(child.Value.ToString(level))
					.Append(" }");
			}
			builder.Append(" }");
			level.Decrement();
			return builder.ToString();
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	public IEnumerator<KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>> GetEnumerator()
	{
		return children.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible.Read(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, Type expectedType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectDeserializer nestedObjectDeserializer)
	{
		Load(parser, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState());
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible.Write(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer nestedObjectSerializer)
	{
		Emit(emitter, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState());
	}

	public static _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode FromObject(object mapping)
	{
		if (mapping == null)
		{
			throw new ArgumentNullException("mapping");
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode();
		foreach (PropertyInfo publicProperty in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReflectionExtensions.GetPublicProperties(mapping.GetType()))
		{
			if (publicProperty.CanRead && publicProperty.GetGetMethod(nonPublic: false).GetParameters().Length == 0)
			{
				object value = publicProperty.GetValue(mapping, null);
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 = value as _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode;
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 == null)
				{
					string text = Convert.ToString(value, CultureInfo.InvariantCulture);
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 = text ?? string.Empty;
				}
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2.Add(publicProperty.Name, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2);
			}
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2;
	}
}
