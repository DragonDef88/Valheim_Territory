using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Schemas;

namespace YamlDotNet.RepresentationModel;

[DebuggerDisplay("{Value}")]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlConvertible
{
	private bool forceImplicitPlain;

	private string? value;

	public string? Value
	{
		get
		{
			return value;
		}
		set
		{
			if (value == null)
			{
				forceImplicitPlain = true;
			}
			else
			{
				forceImplicitPlain = false;
			}
			this.value = value;
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle Style { get; set; }

	public override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType NodeType => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType.Scalar;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		Load(parser, state);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar = parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar>();
		Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar, state);
		string text = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain && base.Tag.IsEmpty)
		{
			forceImplicitPlain = text.Length switch
			{
				0 => true, 
				1 => text == "~", 
				4 => text == "null" || text == "Null" || text == "NULL", 
				_ => false, 
			};
		}
		value = text;
		Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode()
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(string? value)
	{
		Value = value;
	}

	internal override void ResolveAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		throw new NotSupportedException("Resolving an alias on a scalar node does not make sense");
	}

	internal override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag = base.Tag;
		bool isPlainImplicit = tag.IsEmpty;
		if (forceImplicitPlain && Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain && (Value == null || Value == ""))
		{
			tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Null;
			isPlainImplicit = true;
		}
		else if (tag.IsEmpty && Value == null && (Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain || Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any))
		{
			tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Null;
			isPlainImplicit = true;
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(base.Anchor, tag, Value ?? string.Empty, Style, isPlainImplicit, isQuotedImplicit: false));
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		visitor.Visit(this);
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode2 && object.Equals(base.Tag, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode2.Tag))
		{
			return object.Equals(Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode2.Value);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EHashCode.CombineHashCodes(base.Tag.GetHashCode(), Value);
	}

	public static explicit operator string?(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode value)
	{
		return value.Value;
	}

	internal override string ToString(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		return Value ?? string.Empty;
	}

	internal override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> SafeAllNodes(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		yield return this;
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
