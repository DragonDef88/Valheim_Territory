using System;
using System.Collections.Generic;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode
{
	public override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType NodeType => _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNodeType.Alias;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor)
	{
		base.Anchor = anchor;
	}

	internal override void ResolveAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState state)
	{
		throw new NotSupportedException("Resolving an alias on an alias node does not make sense");
	}

	internal override void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state)
	{
		throw new NotSupportedException("A YamlAliasNode is an implementation detail and should never be saved.");
	}

	public override void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		throw new NotSupportedException("A YamlAliasNode is an implementation detail and should never be visited.");
	}

	public override bool Equals(object? obj)
	{
		if (obj is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode2 && Equals(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode2))
		{
			return object.Equals(base.Anchor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode2.Anchor);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal override string ToString(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		return "*" + base.Anchor;
	}

	internal override IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> SafeAllNodes(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ERecursionLevel level)
	{
		yield return this;
	}
}
