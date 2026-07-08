using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Serialization;

namespace YamlDotNet.RepresentationModel;

[DebuggerDisplay("Count = {children.Count}")]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>, IEnumerable, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible
{
	private readonly List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> children = new List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();

	public IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> Children => children;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle Style { get; set; }

	public override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType NodeType => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType.Sequence;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		Load(parser, state);
	}

	private void Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart>();
		Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart, state);
		Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart.Style;
		bool flag = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd>(out @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode.ParseNode(parser, state);
			children.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2);
			flag = flag || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode2 is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode;
		}
		if (flag)
		{
			state.AddNodeWithUnresolvedAliases(this);
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode()
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode(params _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode[] children)
		: this((IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>)children)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> children)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in children)
		{
			this.children.Add(child);
		}
	}

	public void Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child)
	{
		children.Add(child);
	}

	public void Add(string child)
	{
		children.Add(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(child));
	}

	internal override void ResolveAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode)
			{
				children[i] = state.GetNode(children[i].Anchor, children[i].Start, children[i].End);
			}
		}
	}

	internal override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state)
	{
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart(base.Anchor, base.Tag, base.Tag.IsEmpty, Style));
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in children)
		{
			child.Save(emitter, state);
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd());
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override bool Equals(object? obj)
	{
		if (!(obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2) || !object.Equals(base.Tag, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2.Tag) || children.Count != _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2.children.Count)
		{
			return false;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (!object.Equals(children[i], _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2.children[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		int h = 0;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in children)
		{
			h = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(h, child);
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(h, base.Tag);
	}

	internal override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> SafeAllNodes(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		level.Increment();
		yield return this;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in children)
		{
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode item in child.SafeAllNodes(level))
			{
				yield return item;
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
			builder.Append("[ ");
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in children)
			{
				if (builder.Length > 2)
				{
					builder.Append(", ");
				}
				builder.Append(child.ToString(level));
			}
			builder.Append(" ]");
			level.Decrement();
			return builder.ToString();
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	public IEnumerator<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> GetEnumerator()
	{
		return Children.GetEnumerator();
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
}
