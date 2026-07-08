using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode
{
	private const int MaximumRecursionLevel = 1000;

	internal const string MaximumRecursionLevelReachedToStringValue = "WARNING! INFINITE RECURSION!";

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Anchor { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName Tag { get; set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark Start { get; private set; } = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;


	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark End { get; private set; } = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;


	public IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> AllNodes
	{
		get
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel(1000);
			return SafeAllNodes(level);
		}
	}

	public abstract _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType NodeType { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode this[int index]
	{
		get
		{
			if (!(this is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2))
			{
				throw new ArgumentException($"Accessed '{NodeType}' with an invalid index: {index}. Only Sequences can be indexed by number.");
			}
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode2.Children[index];
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode this[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode key]
	{
		get
		{
			if (!(this is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2))
			{
				throw new ArgumentException($"Accessed '{NodeType}' with an invalid index: {key}. Only Mappings can be indexed by key.");
			}
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode2.Children[key];
		}
	}

	internal void Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent yamlEvent, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		Tag = yamlEvent.Tag;
		if (!yamlEvent.Anchor.IsEmpty)
		{
			Anchor = yamlEvent.Anchor;
			state.AddAnchor(this);
		}
		Start = yamlEvent.Start;
		End = yamlEvent.End;
	}

	internal static _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode ParseNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		if (parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar>(out var _))
		{
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(parser, state);
		}
		if (parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart>(out var _))
		{
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode(parser, state);
		}
		if (parser.Accept<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart>(out var _))
		{
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode(parser, state);
		}
		if (parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias>(out var event4))
		{
			if (!state.TryGetNode(event4.Value, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode node))
			{
				return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode(event4.Value);
			}
			return node;
		}
		throw new ArgumentException("The current event is of an unsupported type.", "parser");
	}

	internal abstract void ResolveAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state);

	internal void Save(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state)
	{
		if (!Anchor.IsEmpty && !state.EmittedAnchors.Add(Anchor))
		{
			emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(Anchor));
		}
		else
		{
			Emit(emitter, state);
		}
	}

	internal abstract void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state);

	public abstract void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor);

	public override string ToString()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel(1000);
		return ToString(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel);
	}

	internal abstract string ToString(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level);

	internal abstract IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> SafeAllNodes(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level);

	public static implicit operator _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode(string value)
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(value);
	}

	public static implicit operator _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode(string[] sequence)
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode(((IEnumerable<string>)sequence).Select((Func<string, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>)((string i) => i)));
	}

	public static explicit operator string?(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode node)
	{
		if (!(node is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode2))
		{
			throw new ArgumentException($"Attempted to convert a '{node.NodeType}' to string. This conversion is valid only for Scalars.");
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode2.Value;
	}
}
